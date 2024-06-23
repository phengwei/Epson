<template>
  <div class="d-flex justify-content-center align-items-center vh-100" data-app="true">
    <v-card class="mx-auto card-round" style="width: 90%; padding: 20px;">
      <v-toolbar flat>
        <v-toolbar-title>
          <h2 class="blue-text big-bold">{{ breached ? 'BREACHED REQUESTS' : 'REQUESTS' }}</h2>
        </v-toolbar-title>
        <v-spacer></v-spacer>
        <v-text-field v-model="search"
                      prepend-inner-icon="mdi-magnify"
                      placeholder="Search"
                      solo
                      hide-details
                      flat
                      dense
                      class="search-bar"></v-text-field>
      </v-toolbar>
      <div class="filter-container">
        <div class="month-selector">
          <v-select v-model="selectedMonth" :items="months" @change="getRequests" class="month-select"></v-select>
        </div>
        <div class="create-quotation">
          <v-btn v-if="loggedInUser && loggedInUser.roles.includes('Sales')" class="request-btn" @click="redirectToCreateQuotation">Create Quotation</v-btn>
        </div>
      </div>
      <v-card-text>
        <v-data-table :headers="headers"
                      :items="filteredRequests"
                      :items-per-page="5"
                      :options.sync="options"
                      :loading="loading"
                      class="elevation-1">
          <template v-slot:item.action="{ item }">
            <v-btn @click="viewRequest(item)">View</v-btn>
          </template>
        </v-data-table>
      </v-card-text>
    </v-card>
  </div>
</template>

<script>
  import { mapGetters } from 'vuex';
  import moment from 'moment';
  import { ApprovalStateEnum } from '~/script/approvalStateEnum.js';
  import { RequestProductStatusEnum } from '~/script/requestProductStatusEnum.js';

  export default {
    name: 'RequestOverview',
    computed: {
      ...mapGetters(['isAuthenticated', 'loggedInUser']),
      filteredRequests() {
        let filtered = this.requests;
        if (this.search.trim() !== '') {
          filtered = filtered.filter(request => {
            const requestIdMatch = String(request.id).toLowerCase().includes(this.search.toLowerCase());
            const projectNameMatch = request.projectInformationModel &&
              request.projectInformationModel.projectName &&
              request.projectInformationModel.projectName.toLowerCase().includes(this.search.toLowerCase());
            return requestIdMatch || projectNameMatch;
          });
        }
        if (this.selectedMonth !== null) {
          filtered = filtered.filter(request => {
            const requestMonth = moment(request.createdOnUTC).month() + 1; // months are 0-indexed
            return this.selectedMonth === 0 || requestMonth === this.selectedMonth;
          });
        }
        return filtered;
      }
    },
    data() {
      return {
        headers: [
          { text: 'Request #', value: 'id' },
          { text: 'End User', value: 'endUserName' },
          { text: 'Approval State', value: 'approvalStateStr' },
          { text: 'Total Budget (RM)', value: 'totalBudget' },
          { text: 'Created On', value: 'createdOnUTC' },
          { text: 'Created By', value: 'createdBy' },
          { text: 'Approved Time', value: 'approvedTime' },
          { text: 'Requester Team', value: 'createdByTeam' },
          { text: 'Actions', value: 'action' }
        ],
        requests: [],
        options: {},
        loading: false,
        search: '',
        breached: false,
        selectedMonth: new Date().getMonth() + 1,
        months: [
          { value: 0, text: 'All' },
          { value: 1, text: 'January' },
          { value: 2, text: 'February' },
          { value: 3, text: 'March' },
          { value: 4, text: 'April' },
          { value: 5, text: 'May' },
          { value: 6, text: 'June' },
          { value: 7, text: 'July' },
          { value: 8, text: 'August' },
          { value: 9, text: 'September' },
          { value: 10, text: 'October' },
          { value: 11, text: 'November' },
          { value: 12, text: 'December' }
        ],
        ApprovalStateEnum,
        RequestProductStatusEnum,
      };
    },
    created() {
      this.breached = this.$route.query.breached === 'true';
      this.selectedMonth = this.$route.query.month ? parseInt(this.$route.query.month) : new Date().getMonth() + 1;
      this.getRequests();
    },
    methods: {
      getRequests() {
        const params = { breached: this.breached };
        this.$axios.get(`${this.$config.restUrl}/api/request/getrequests`, { params })
          .then(response => {
            this.requests = response.data.data.map(item => {
              const isApproved = item.approvalState === this.ApprovalStateEnum.Approved;
              const approvedTime = isApproved ? moment(item.approvedTime).add(8, 'hours').format('DD MMM YY HH:mm') : 'N/A';
              return {
                ...item,
                endUserName: item.projectInformationModel.projectName || 'N/A',
                createdOnUTC: moment(item.createdOnUTC).format('DD MMM YY HH:mm'),
                approvedTime,
                createdByTeam: item.createdTeam || 'N/A'
              };
            });
          })
          .catch(error => {
            console.error('Error fetching requests:', error);
          });
      },
      redirectToCreateQuotation() {
        this.$router.push('/createquotation?create=true');
      },
      viewRequest(request) {
        const anyProductRejected = request.requestProductsModel.some(product =>
          product.status === this.RequestProductStatusEnum.Rejected
        );
        let queryParameters = { request: JSON.stringify(request), month: this.selectedMonth };
        if (this.loggedInUser && this.loggedInUser.roles.includes('Sales Section Head')
          && request.approvalState === this.ApprovalStateEnum.PendingSalesSectionHeadAction
          && request.createdById !== this.loggedInUser.id) {
          queryParameters = { ...queryParameters, isApprove: true, view: true };
        } else if (this.loggedInUser && this.loggedInUser.roles.includes('Sales Section Head')
          && request.approvalState === this.ApprovalStateEnum.PendingSalesSectionHeadFinalAction) {
          queryParameters = { ...queryParameters, isFinalApprove: true, view: true };
        } else if (this.loggedInUser && this.loggedInUser.id === request.createdById
          && request.approvalState === this.ApprovalStateEnum.PendingRequesterAction) {
          queryParameters = { ...queryParameters, dealable: true, view: true, amendable: true };
        } else if (this.loggedInUser && this.loggedInUser.id === request.createdById
          && request.approvalState === this.ApprovalStateEnum.PendingFulfillerAction
          && anyProductRejected) {
          queryParameters = { ...queryParameters, amendable: true, view: true };
        } else if (this.loggedInUser && this.loggedInUser.id === request.createdById
          && request.approvalState === this.ApprovalStateEnum.AmendQuotation) {
          queryParameters = { ...queryParameters, editable: true };
        } else if (this.loggedInUser && this.loggedInUser.id === request.createdById
          && request.approvalState === this.ApprovalStateEnum.RejectedByFulfiller) {
          queryParameters = { ...queryParameters, amendable: true, view: true };
        } else {
          queryParameters = { ...queryParameters, view: true };
        }

        this.$router.push({
          path: '/createquotation',
          query: queryParameters
        });
      },
    },
  };
</script>

<style scoped>
  @import '~@/../wwwroot/css/general-table.css';

  .filter-container {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 16px;
  }

  .month-selector {
    width: 20%;
    min-width: 150px;
  }

  .create-quotation {
    margin-left: 16px;
  }

  .vh-100 {
    height: 100vh;
  }

  .form-group {
    margin-bottom: 1rem;
    display: flex;
    justify-content: center;
    flex-direction: column;
  }

  label {
    font-weight: bold;
    margin-bottom: 0.5rem;
    color: black;
  }

  .border-input {
    border: 1px solid #ccc;
    border-radius: 10px;
    padding: 0.5rem;
    width: 100%;
  }

  .role-checkbox {
    display: flex;
    align-items: center;
    margin-bottom: 0.5rem;
  }

    .role-checkbox input[type="checkbox"] {
      margin-right: 0.5rem;
    }

      .role-checkbox input[type="checkbox"].styled-checkbox {
        appearance: none;
        width: 16px;
        height: 16px;
        border: 1px solid #003399;
        border-radius: 4px;
        position: relative;
        cursor: pointer;
      }

        .role-checkbox input[type="checkbox"].styled-checkbox:checked::before {
          font-size: 12px;
          color: #003399;
          position: absolute;
          top: 1px;
          left: 2px;
        }

  .search-bar {
    width: 150px;
    border-radius: 20px;
    padding: 5px 10px;
    border-width: medium;
  }

    .search-bar .v-input__control {
      background-color: #d3d3d3;
    }

  .theme--light.v-text-field--solo > .v-input__control > .v-input__slot {
    background-color: #d3d3d3 !important;
  }

  .search-bar .v-field__append-inner,
  .search-bar .v-field__prepend-inner {
    margin-top: 0;
  }

  .search-bar .v-input__control {
    border: none;
  }

  .blue-button {
    background-color: #003399 !important;
    color: white !important;
  }

  .blue-text {
    color: #003399 !important;
  }

  .blue-text--active {
    color: #003399 !important;
  }

  .big-bold {
    font-size: 24px;
    font-weight: bold;
  }

  .small-bold {
    font-size: 16px;
    font-weight: bold;
  }

  .card-round {
    border-radius: 20px !important;
  }

  .table-padding {
    padding: 20px;
  }
</style>
