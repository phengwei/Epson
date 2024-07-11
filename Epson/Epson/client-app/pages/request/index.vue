<template>
  <div class="d-flex justify-content-center align-items-center vh-100" :key="routeKey" data-app="true">
    <v-card class="mx-auto card-round" style="width: 90%; padding: 20px;">
      <v-toolbar flat>
        <v-toolbar-title>
          <h2 class="blue-text big-bold">{{ breached ? 'BREACHED REQUESTS' : 'REQUESTS' }}</h2>
        </v-toolbar-title>
        <v-spacer></v-spacer>
        <v-text-field v-model="searchTerm"
                      append-icon="mdi-magnify"
                      placeholder="Search by end user or request #"
                      solo
                      hide-details
                      flat
                      dense
                      class="search-bar"
                      @keyup.enter="triggerSearch"
                      @click:append="triggerSearch"></v-text-field>
      </v-toolbar>
      <div class="filter-container">
        <div class="month-selector">
          <v-select v-model="selectedMonth" :items="months" @change="triggerSearch" class="month-select"></v-select>
        </div>
        <div class="create-quotation">
          <v-btn v-if="loggedInUser && loggedInUser.roles.includes('Sales')" class="request-btn" @click="redirectToCreateQuotation">Create Quotation</v-btn>
        </div>
      </div>
      <v-card-text>
        <v-data-table :headers="headers"
                      :items="requests"
                      :options.sync="options"
                      :items-per-page="options.itemsPerPage"
                      :footer-props="{ itemsPerPageOptions: [5, 10, 20, 30, 50, { text: 'All', value: -1 }], 'items-per-page-text': 'Rows per page:', 'items-per-page-align': 'right' }"
                      :loading="loading"
                      :server-items-length="totalItems"
                      @update:options="updateOptions"
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
  import { Base64 } from 'js-base64';
  import moment from 'moment';
  import { ApprovalStateEnum } from '~/script/approvalStateEnum.js';
  import { RequestProductStatusEnum } from '~/script/requestProductStatusEnum.js';

  const approvalStateMapping = {
    [ApprovalStateEnum.PendingSalesSectionHeadAction]: 'Pending Sales Section Head Action',
    [ApprovalStateEnum.PendingFulfillerAction]: 'Pending Fulfiller Action',
    [ApprovalStateEnum.PendingRequesterAction]: 'Pending Requester Action',
    [ApprovalStateEnum.PendingSalesSectionHeadFinalAction]: 'Pending Sales Section Head Final Action',
    [ApprovalStateEnum.Approved]: 'Approved',
    [ApprovalStateEnum.AmendQuotation]: 'Amend Quotation',
    [ApprovalStateEnum.RejectedByFulfiller]: 'Rejected By Fulfiller',
    [ApprovalStateEnum.RejectedByRequester]: 'Rejected By Requester',
    [ApprovalStateEnum.RejectedBySalesSectionHead]: 'Rejected By Sales Section Head',
    [ApprovalStateEnum.Cancelled]: 'Cancelled',
    [ApprovalStateEnum.DealExited]: 'Deal Exited'
  };

  export default {
    name: 'RequestOverview',
    props: ['routeKey'],
    data() {
      return {
        headers: [
          { text: 'Request #', value: 'id', align: 'center', sortable: false },
          { text: 'End User', value: 'endUserName', align: 'center', sortable: false },
          { text: 'Approval State', value: 'approvalStateStr', align: 'center', sortable: false },
          { text: 'Total Budget (RM)', value: 'totalEndUserBudget', align: 'center', sortable: false },
          { text: 'Created On', value: 'createdOnUTC', align: 'center', sortable: false },
          { text: 'Created By', value: 'createdByStr', align: 'center', sortable: false },
          { text: 'Approved Time', value: 'approvedTime', align: 'center', sortable: false },
          { text: 'Requester Team', value: 'teamName', align: 'center', sortable: false },
          { text: 'Actions', value: 'action', align: 'center', sortable: false }
        ],
        requests: [],
        options: {
          page: 1,
          itemsPerPage: 10,
          sortBy: [],
          sortDesc: [],
        },
        totalItems: 0,
        loading: true,
        searchTerm: '',
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
    computed: {
      ...mapGetters(['isAuthenticated', 'loggedInUser']),
    },
    watch: {
      options: {
        handler() {
          this.getRequests();
        },
        deep: true,
      },
      $route(to, from) {
        if (to.fullPath !== from.fullPath) {
          this.onRouteChange();
        }
      }
    },
    created() {
      this.breached = this.$route.query.breached === 'true';
      this.selectedMonth = this.$route.query.month ? parseInt(this.$route.query.month) : new Date().getMonth() + 1;
      this.getRequests();
    },
    mounted() {
      this.modifySelectInputs();
      this.modifyTextInputs();
    },
    methods: {
      modifyTextInputs() {
        const vSelects = this.$el.querySelectorAll('.v-text-field__slot');
        vSelects.forEach(vSelect => {
          const inputElement = vSelect.querySelector('input[type="text"]');
          if (inputElement) {
            inputElement.removeAttribute('type');
          }
        });
      },
      modifySelectInputs() {
        const vSelects = this.$el.querySelectorAll('.v-select');
        vSelects.forEach(vSelect => {
          const inputElement = vSelect.querySelector('input[type="text"]');
          if (inputElement) {
            inputElement.removeAttribute('type');
          }
        });
      },
      onRouteChange() {
        this.$nextTick(() => {
          this.getRequests();
        });
      },
      getRequests() {
        this.loading = true;
        const params = {
          search: this.search,
          breached: this.breached,
          page: this.options.page,
          itemsPerPage: this.options.itemsPerPage,
          month: this.selectedMonth,
        };
        this.$axios.get(`${this.$config.restUrl}/api/request/getrequests`, { params })
          .then(response => {
            console.log("response", response);
            this.requests = response.data.data.map(item => {
              const isApproved = item.approvalState === this.ApprovalStateEnum.Approved;
              const approvedTime = isApproved ? moment(item.approvedTime).add(8, 'hours').format('DD MMM YY HH:mm') : 'N/A';
              return {
                ...item,
                totalEndUserBudget: item.projectInformation.budget,
                approvalStateStr: approvalStateMapping[item.approvalState] || 'Pending',
                endUserName: item.projectInformation.projectName || 'N/A',
                createdOnUTC: moment(item.createdOnUTC).format('DD MMM YY HH:mm'),
                approvedTime
              };
            });
            this.totalItems = response.data.count;
            this.loading = false;
          })
          .catch(error => {
            this.loading = false;
            console.error('Error fetching requests:', error);
          });
      },
      triggerSearch() {
        this.search = this.searchTerm;
        this.options.page = 1;
        this.getRequests();
      },
      updateOptions(options) {
        this.options = options;
        this.getRequests();
      },
      redirectToCreateQuotation() {
        this.$router.push('/createquotation?create=true');
      },
      viewRequest(request) {
        console.log("request", request);
        const anyProductRejected = request.requestProducts.some(product =>
          product.status === this.RequestProductStatusEnum.Rejected
        );

        let queryParameters = { requestId: request.id, month: this.selectedMonth };

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
          && request.approvalState === this.RequestProductStatusEnum.Rejected) {
          queryParameters = { ...queryParameters, amendable: true, view: true };
        } else {
          queryParameters = { ...queryParameters, view: true };
        }

        const encodedParams = Base64.encode(JSON.stringify(queryParameters));

        this.$router.push({
          path: '/createquotation',
          query: { params: encodedParams }
        });
      },
    },
  };
</script>

<style scoped>
  @import '~@/../wwwroot/css/general-table.css';

  v-btn {
    background-color: #003399 !important;
  }

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
      background-color: #003399;
    }

  .theme--light.v-btn.v-btn--has-bg {
    background-color: #003399 !important;
    color: white !important;
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
