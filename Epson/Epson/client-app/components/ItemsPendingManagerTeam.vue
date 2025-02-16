<template>
  <v-app>
    <v-data-table :headers="headers"
                  :items="requests"
                  :options.sync="options"
                  :items-per-page="options.itemsPerPage"
                  :loading="loading"
                  :footer-props="{ 'items-per-page-options': [5, 10, 20, 30, 50, { text: 'All', value: -1 }] }"
                  :server-items-length="totalItems"
                  class="elevation-1">
      <template v-slot:top>
        <v-toolbar flat>
          <v-toolbar-title class="blue-text big-bold">REQUESTS PENDING MANAGER TEAM APPROVAL</v-toolbar-title>
          <v-spacer></v-spacer>
          <v-text-field v-model="searchTerm"
                        append-icon="mdi-magnify"
                        placeholder="Search by request # or requested by"
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
            <input v-model="editedItem.createdOnUTC" class="border-input readonly-field" disabled />
          </div>
          <div class="form-group">
            <label>Total Budget</label>
            <input v-model="editedItem.totalBudget" class="border-input readonly-field" disabled />
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

  export default {
    name: 'PendingManagerTeamRequests',
    data() {
      return {
        error: null,
        dialog: false,
        headers: [
          { text: 'Request #', align: 'start', value: 'id', sortable: false },
          { text: 'Owner', align: 'start', value: 'owner', sortable: false },
          { text: 'Status', align: 'start', value: 'status', sortable: false },
          { text: 'Reported Date', align: 'start', value: 'reportedDate', sortable: false },
          { text: 'Requester', align: 'start', value: 'requester', sortable: false },
          { text: 'Category', align: 'start', value: 'category', sortable: false },
          { text: 'Created On', align: 'start', value: 'createdOnUTC', sortable: false },
          { text: 'Actions', value: 'actions', sortable: false },
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
        }
      };
    },
    computed: {
      formTitle() {
        return 'Request Details';
      }
    },
    watch: {
      options: {
        handler() {
          this.getPendingManagerTeamRequests();
        },
        deep: true,
      },
    },
    created() {
      this.getPendingManagerTeamRequests();
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
        this.$router.push({
          path: '/serviceQuotation',
          query: {
            id: request.id,
            view: true
          },
        });
      },

      /**
       * Fetches Pending Manager Team Requests from API
       */
      getPendingManagerTeamRequests() {
        this.loading = true;
        this.$axios.get(`${this.$config.restUrl}/api/serviceRequest/GetPendingManagerTeamRequests`)
          .then(result => {
            this.requests = result.data.data.map(request => ({
              ...request,
              createdOnUTC: moment(request.createdOnUTC).format('DD MMM YY HH:mm'),
              reportedDate: moment(request.reportedDate).format('DD MMM YY HH:mm')
            }));
            this.totalItems = this.requests.length;
            this.loading = false;
          })
          .catch(error => {
            this.loading = false;
            console.error('Error fetching pending manager team requests:', error);
          });
      },

      /**
       * Search Requests
       */
      triggerSearch() {
        this.getPendingManagerTeamRequests();
      },

      /**
       * Approve or Save Request Changes
       */
      async close() {
        try {
          const result = await this.$axios.post(
            `${this.$config.restUrl}/api/request/approverequest?id=${this.editedItem.id}&comments=${this.editedItem.comments}`
          );

          if (result.status === 200) {
            Swal.fire({
              icon: 'success',
              title: 'Saved successfully!',
              showConfirmButton: false,
              timer: 1500
            });

            this.getPendingManagerTeamRequests();
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
    }
  };
</script>


<style scoped>
  @import '~@/../wwwroot/css/general-table.css';
</style>
