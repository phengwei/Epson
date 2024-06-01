<template>
  <div class="d-flex justify-content-center align-items-center vh-100" data-app="true">
    <v-card class="mx-auto" style="width: 90%">
      <v-card-title class="d-flex justify-content-between align-items-center">
        <span style="flex-grow: 1;">{{ breached ? 'Breached Request' : 'Request' }}</span>
        <div class="d-flex align-items-center">
          <v-text-field v-model="search"
                        class="search-input"
                        append-icon="mdi-magnify"
                        label="Search by end user or request #"
                        single-line
                        hide-details></v-text-field>
          <v-select v-model="selectedMonth" :items="months" @change="getRequests" class="month-select"></v-select>
        </div>
        <v-btn v-if="loggedInUser && loggedInUser.roles.includes('Sales')" class="request-btn" @click="redirectToCreateQuotation">Create Quotation</v-btn>
      </v-card-title>
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
  .vh-100 {
    height: 100vh;
  }

  .request-btn {
    background-color: #272727 !important;
    color: white !important;
    border: none;
    margin-left: 16px;
    padding: 10px 16px;
    font-size: 14px;
    border-radius: 4px;
  }

  .search-input {
    flex-grow: 1;
    margin-left: 16px;
    margin-right: 16px;
    width: auto;
  }

  .filter-container {
    display: flex;
    align-items: center;
    margin-right: 16px;
  }

  .month-select {
    margin-left: 16px;
    min-width: 150px;
  }
</style>
