<template>
  <div class="d-flex justify-content-center align-items-center vh-100" data-app="true" v-if="loggedInUser.roles.includes('Sales') || loggedInUser.roles.includes('Sales Section Head')">
    <v-card class="mx-auto" style="width: 90%">
      <v-card-title class="d-flex justify-content-between align-items-center">
        <span style="flex-grow: 1;">Request</span>
        <v-btn class="request-btn" @click="redirectToCreateQuotation">Create Quotation</v-btn>
      </v-card-title>
      <v-card-text>
        <v-data-table :headers="headers"
                      :items="requests"
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
      ...mapGetters(['isAuthenticated', 'loggedInUser'])
    },
    data() {
      return {
        headers: [
          { text: 'Request #', value: 'id' },
          { text: 'Approval State', value: 'approvalStateStr' },
          { text: 'Total Budget (RM)', value: 'totalBudget' },
          { text: 'Created On', value: 'createdOnUTC' },
          { text: 'Actions', value: 'action' }
        ],
        requests: [],
        options: {},
        loading: false,
        ApprovalStateEnum,
        RequestProductStatusEnum,
      };
    },
    created() {
      this.getRequests();
    },
    methods: {
      getRequests() {
        this.$axios.get(`${this.$config.restUrl}/api/request/getrequests`)
          .then(response => {
            this.requests = response.data.data.map(item => ({
              ...item,
              createdOnUTC: moment(item.createdOnUTC).format('DD MMM YY HH:mm')    
            }));
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
        let queryParameters = { request: JSON.stringify(request) };
        if (this.loggedInUser.roles.includes('Sales Section Head')
          && request.approvalState === this.ApprovalStateEnum.PendingSalesSectionHeadAction) {
          queryParameters = { ...queryParameters, isApprove: true, view: true };
        } else if (this.loggedInUser.roles.includes('Sales Section Head')
          && request.approvalState === this.ApprovalStateEnum.PendingSalesSectionHeadFinalAction) {
          queryParameters = { ...queryParameters, isFinalApprove: true, view: true };
        } else if (this.loggedInUser.id === request.createdById
          && request.approvalState === this.ApprovalStateEnum.PendingRequesterAction) {
          queryParameters = { ...queryParameters, dealable: true, view: true, amendable: true };
        } else if (this.loggedInUser.id === request.createdById
          && request.approvalState === this.ApprovalStateEnum.PendingFulfillerAction
          && anyProductRejected) {
          queryParameters = { ...queryParameters, amendable: true, view: true };
        } else if (this.loggedInUser.id === request.createdById
          && request.approvalState === this.ApprovalStateEnum.AmendQuotation) {
          queryParameters = { ...queryParameters, editable: true };
        } else if (this.loggedInUser.id === request.createdById
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
</style>
