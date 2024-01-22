<template>
  <v-app>
    <div class="table-container">
      <v-data-table :headers="headers"
                    :items="requests"
                    :options.sync="options"
                    :items-per-page="5"
                    :loading="loading"
                    class="elevation-1">
        <template v-slot:top>
          <v-toolbar flat>
            <v-toolbar-title>Request Responded</v-toolbar-title>
            <v-divider class="mx-4" inset vertical></v-divider>
            <v-spacer></v-spacer>
          </v-toolbar>
        </template>
        <template v-slot:item.actions="{ item }">
          <v-btn @click="viewRequest(item)">Action</v-btn>
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
              <label>Customer's Expected Pricing'</label>
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
    </div>
  </v-app>
</template>

<script>
  import Swal from 'sweetalert2'
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
        options: {},
        requests: [],
        loading: false,
        editedIndex: -1,
        editedItem: {
          id: 0,
          name: '',
          price: 0,
          comments: ''
        },
        ApprovalStateEnum
      }
    },
    computed: {
      formTitle() {
        return 'Request'
      }
    },
    watch: {
      options: {
        handler() {
          this.getPendingRequesterItem()
        },
        deep: true,
      },
    },
    methods: {
      viewRequest(request) {
        console.log("req", request);
        let queryParameters = { view: true, request: JSON.stringify(request) };

        if (request.approvalState === ApprovalStateEnum.PendingRequesterAction) {
          queryParameters = { ...queryParameters, dealable: true };
        } else if (request.approvalState === ApprovalStateEnum.RejectedByFulfiller) {
          queryParameters = { ...queryParameters, amendable: true };
        }

        this.$router.push({
          path: '/createquotation',
          query: queryParameters
        });
      },
      getPendingRequesterItem() {
        this.loading = true
        this.$axios.get(`${this.$config.restUrl}/api/request/getpendingrequesteritem`).then(result => {
          for (const requests in result.data.data) {
            result.data.data[requests].createdOnUTC = moment(this.editedItem.createdOnUTC).format('DD MMM YY HH:mm');
          }
          this.requests = result.data.data
          this.loading = false
        })
      },
      editItem(item) {
        this.editedIndex = this.requests.indexOf(item)
        this.editedItem = { ...item };
        this.editedItem.requestProductsModel.forEach(product => {
          product.fulfilledDate = moment(product.fulfilledDate).format('DD MMM YY HH:mm');
          product.tenderDate = moment(product.tenderDate).format('DD MMM YY HH:mm');
          product.deliveryDate = moment(product.deliveryDate).format('DD MMM YY HH:mm');
        });
        this.dialog = true
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
            this.getPendingRequesterItem();
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
            this.getPendingRequesterItem();
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
  .table-container {
    display: flex;
    justify-content: center;
    align-items: center;
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
    border-radius: 4px;
    padding: 0.5rem;
    width: 100%;
  }

  .blue-checkbox {
    margin-bottom: 1rem;
  }

    .blue-checkbox input[type="checkbox"]:checked {
      background-color: #4285f4;
      border-color: #4285f4;
    }

  input[type="checkbox"] {
    margin-right: 0.5rem;
    -webkit-appearance: none;
    -moz-appearance: none;
    appearance: none;
    border-radius: 3px;
    border: 2px solid #ccc;
    width: 1.2em;
    height: 1.2em;
    margin-left: 5%
  }

  .readonly-field {
    background-color: #ddd;
  }
</style>
