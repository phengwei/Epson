<template>
  <v-app>
    <v-data-table :headers="headers"
                  :items="filteredRequests"
                  :options.sync="options"
                  :items-per-page="options.itemsPerPage"
                  :loading="loading"
                  :footer-props="{ 'items-per-page-options': [5, 10, 20, 30, 50, { text: 'All', value: -1 }] }"
                  :server-items-length="totalItems"
                  class="elevation-1">
      <template v-slot:top>
        <v-toolbar flat>
          <v-toolbar-title class="blue-text big-bold">REQUESTS PENDING FULFILMENT</v-toolbar-title>
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
      </template>
      <template v-slot:item.actions="{ item }">
        <v-btn @click="viewRequest(item)">View</v-btn>
      </template>
    </v-data-table>

    <v-dialog v-model="dialog" max-width="800px">
      <v-card>
        <v-card-title>
          <span class="text-h5">{{ formTitle }}</span>
        </v-card-title>
        <v-card-text>
          <div class="form-group">
            <label>Created On</label>
            <input v-model="editedItem.createdOnUTC" class="border-input readonly-field" label="Date" disabled></input>
          </div>
          <div class="form-group">
            <label>Customer's Expected Pricing</label>
            <input v-model="editedItem.totalPrice" class="border-input readonly-field" label="Price" disabled></input>
          </div>
          <div class="form-group">
            <label>Project Budget</label>
            <input v-model="editedItem.totalBudget" class="border-input readonly-field" label="Budget" disabled></input>
          </div>
          <div class="form-group">
            <label>Comments (if any)</label>
            <textarea v-model="editedItem.comments" class="border-input"></textarea>
          </div>
          <div class="table-container">
            <v-data-table :headers="productHeaders"
                          :items="editedItem.requestProductsModel"
                          :items-per-page="5"
                          class="elevation-1">
              <template v-slot:top>
                <v-toolbar flat>
                  <v-toolbar-title>Requested Product Details</v-toolbar-title>
                </v-toolbar>
              </template>
            </v-data-table>
          </div>
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="blue darken-1" text @click="close">Save</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-app>
</template>

<script>
  import Swal from 'sweetalert2';
  import moment from 'moment';
  import { ApprovalStateEnum } from '~/script/approvalStateEnum.js';

  export default {
    name: 'ItemsPendingRequesterAction',
    data() {
      return {
        error: null,
        dialog: false,
        dialogDelete: false,
        headers: [
          {
            text: 'Request #',
            align: 'start',
            value: 'id',
          },
          {
            text: 'Requested By',
            align: 'start',
            value: 'createdBy',
          },
          {
            text: 'End User',
            align: 'start',
            value: 'endUserName'
          },
          {
            text: 'Created Time',
            align: 'start',
            value: 'createdOnUTC',
          },
          {
            text: 'Total Budget',
            align: 'start',
            value: 'totalBudget',
          },
          { text: 'Record', value: 'actions', sortable: false },
        ],
        productHeaders: [
          { text: 'Product Name', align: 'start', value: 'productName' },
          { text: 'Quantity', align: 'start', value: 'quantity' },
          { text: 'Fulfiller Name', align: 'start', value: 'fulfillerName' },
          { text: 'Fulfilled Date', align: 'start', value: 'fulfilledDate' },
          { text: 'Fulfilled Price', align: 'start', value: 'fulfilledPrice' },
          { text: 'Remarks', align: 'start', value: 'remarks' },
          { text: 'Tender Date', align: 'start', value: 'tenderDate' },
          { text: 'Delivery Date', align: 'start', value: 'deliveryDate' },
        ],
        options: {
          page: 1,
          itemsPerPage: 10,
          sortBy: [],
          sortDesc: [],
        },
        requests: [],
        totalItems: 0,
        loading: true,
        searchTerm: '',
        search: '',
        editedIndex: -1,
        editedItem: {
          id: 0,
          name: '',
          price: 0,
          comments: ''
        },
        ApprovalStateEnum
      };
    },
    computed: {
      formTitle() {
        return 'Request';
      },
      filteredRequests() {
        if (!this.search || this.search.trim() === '') {
          return this.requests;
        }
        return this.requests.filter(request => {
          const requestIdMatch = String(request.id).toLowerCase().includes(this.search.toLowerCase());
          const projectNameMatch = request.projectInformationModel &&
            request.projectInformationModel.projectName &&
            request.projectInformationModel.projectName.toLowerCase().includes(this.search.toLowerCase());
          return requestIdMatch || projectNameMatch;
        });
      }
    },
    watch: {
      options: {
        handler() {
          this.getPendingSalesSectionHeadItem();
        },
        deep: true,
      },
    },
    created() {
      this.getPendingSalesSectionHeadItem();
    },
    mounted() {
      this.modifySelectInputs();
    },
    methods: {
      modifySelectInputs() {
        const vSelects = this.$el.querySelectorAll('.v-text-field__slot');
        vSelects.forEach(vSelect => {
          const inputElement = vSelect.querySelector('input[type="text"]');
          if (inputElement) {
            inputElement.removeAttribute('type');
          }
        });
      },
      viewRequest(request) {
        let queryParameters = { view: true, request: JSON.stringify(request) };

        if (request.approvalState === ApprovalStateEnum.PendingSalesSectionHeadAction) {
          queryParameters = { ...queryParameters, isApprove: true };
        }

        this.$router.push({
          path: '/createquotation',
          query: queryParameters
        });
      },
      getPendingSalesSectionHeadItem() {
        this.loading = true;
        const params = {
          search: this.search,
          page: this.options.page,
          itemsPerPage: this.options.itemsPerPage,
        };
        this.$axios.get(`${this.$config.restUrl}/api/request/getpendingsalessectionheaddepartmentrequests`, { params }).then(result => {
          this.requests = result.data.data.map(request => {
            return {
              ...request,
              endUserName: request.projectInformationModel.projectName || 'N/A',
              createdOnUTC: moment(request.createdOnUTC).format('DD MMM YY HH:mm')
            };
          });
          this.totalItems = result.data.count;
          this.loading = false;
        }).catch(error => {
          this.loading = false;
          console.error('Error fetching pending sales section head items:', error);
        });
      },
      editItem(item) {
        this.editedIndex = this.requests.indexOf(item);
        this.editedItem = { ...item };
        this.editedItem.requestProductsModel.forEach(product => {
          product.fulfilledDate = moment(product.fulfilledDate).format('DD MMM YY HH:mm');
          product.tenderDate = moment(product.tenderDate).format('DD MMM YY HH:mm');
          product.deliveryDate = moment(product.deliveryDate).format('DD MMM YY HH:mm');
        });
        this.dialog = true;
      },
      triggerSearch() {
        this.search = this.searchTerm;
        this.options.page = 1;
        this.getPendingSalesSectionHeadItem();
      },
      async close() {
        try {
          const result = await this.$axios.post(`${this.$config.restUrl}/api/request/approverequest?id=${this.editedItem.id}&comments=${this.editedItem.comments}`);
          if (result.status === 200) {
            Swal.fire({
              icon: 'success',
              title: 'Saved successfully!',
              showConfirmButton: false,
              timer: 1500
            });
            this.getPendingSalesSectionHeadItem();
            this.dialog = false;
          }
        } catch (err) {
          console.log(err);
          Swal.fire({
            icon: 'error',
            title: 'Oops...',
            text: err.response ? err.response.data.message : "An error occurred!"
          });
        }
      },
      async save() {
        try {
          const result = await this.$axios.post(`${this.$config.restUrl}/api/request/approverequest?id=${this.editedItem.id}`);
          if (result.status === 200) {
            Swal.fire({
              icon: 'success',
              title: 'Deal lost successfully!',
              showConfirmButton: false,
              timer: 1500
            });
            this.getPendingSalesSectionHeadItem();
            this.dialog = false;
          }
        } catch (err) {
          console.log(err);
          Swal.fire({
            icon: 'error',
            title: 'Oops...',
            text: err.response ? err.response.data.message : "An error occurred!"
          });
        }
      }
    },
  }
</script>

<style scoped>
  @import '~@/../wwwroot/css/general-table.css';
</style>
