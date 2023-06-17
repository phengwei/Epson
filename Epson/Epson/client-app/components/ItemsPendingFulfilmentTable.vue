<template>
  <v-data-table :headers="headers"
                :items="itemsPendingFulfilment"
                :loading="loading"
                class="elevation-1">
    <template v-slot:top>
      <v-toolbar flat>
        <v-toolbar-title>New Request</v-toolbar-title>
        <v-divider class="mx-4" inset vertical></v-divider>
        <v-spacer></v-spacer>

        <v-dialog v-model="dialog" max-width="800px">
          <v-card>
            <v-card-title>
              <span class="text-h5">RESPONDENT - Product Managers</span>
            </v-card-title>
            <v-card-text>
              <div class="form-group">
                <label>Requested By</label>
                <input v-model="editedItem.createdBy" class="border-input readonly-field" label="Requested By" readonly></input>
              </div>
              <div class="form-group">
                <label>Customer</label>
                <input v-model="editedItem.customerName" class="border-input readonly-field" label="Customer" readonly></input>
              </div>
              <div v-if="competitorsToShow && competitorsToShow.length > 0">
                <label>Competitor Information</label>
                <table class="mb-5 mt-2 mini-table">
                  <thead>
                    <tr>
                      <th>Model</th>
                      <th>Brand</th>
                      <th>Price</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="(competitor, index) in competitorsToShow" :key="index">
                      <td>{{ competitor.model }}</td>
                      <td>{{ competitor.brand }}</td>
                      <td>{{ competitor.price }}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
              <div class="form-group">
                <label>Product</label>
                <input v-model="editedItem.productName" class="border-input readonly-field" label="Product" readonly></input>
              </div>
              <div class="form-group">
                <label>Project End User Price</label>
                <input v-model="editedItem.budget" class="border-input readonly-field" label="Budget" readonly></input>
              </div>
              <div class="form-group">
                <label>Quantity</label>
                <input v-model="editedItem.quantity" class="border-input readonly-field" label="Quantity" readonly></input>
              </div>
              <div class="form-group">
                <label>Requirements</label>
                <input v-model="editedItem.dealJustification" class="border-input readonly-field" label="Requirements" readonly></input>
              </div>
              <div class="form-group">
                <label>Remarks</label>
                <input v-model="editedItem.remarks" class="border-input" label="Remarks"></input>
              </div>
              <div class="form-group">
                <label>Tender Date</label>
                <input type="datetime-local" v-model="editedItem.tenderDate" class="border-input" :min="today()" label="Tender Date" readonly></input>
              </div>
              <div class="form-group">
                <label>Delivery Date</label>
                <input type="datetime-local" v-model="editedItem.deliveryDate" class="border-input" :min="today()" label="Delivery Date"></input>
              </div>
              <div class="form-group">
                <label>Dealer Price</label>
                <input v-model="editedItem.fulfilledPrice" type="number" class="border-input" label="Approved Price" required></input>
              </div>

              <v-card-actions>
                <v-spacer></v-spacer>
                <v-btn color="blue darken-1" text @click="closeDialog">Cancel</v-btn>
                <v-btn color="blue darken-1" text @click="fulfillRequest">Approve</v-btn>
              </v-card-actions>
            </v-card-text>
          </v-card>
        </v-dialog>
      </v-toolbar>
    </template>

    <template v-slot:item.actions="{ item }">
      <v-icon small
              class="mr-2"
              @click="editItem(item)">
        mdi-pencil
      </v-icon>
    </template>
  </v-data-table>
</template>

<script>
  import Swal from 'sweetalert2';
  export default {
    name: 'ItemsPendingFulfilmentTable',
    data() {
      return {
        dialog: false,
        headers: [
          {
            text: 'ID',
            align: ' d-none',
            value: 'id',
          },
          { text: 'Requested By', value: 'createdBy' },
          { text: 'Customer', value: 'customerName' },
          { text: 'Product', value: 'productName' },
          { text: 'Budget', value: 'budget' },
          { text: 'Quantity', value: 'quantity' },
          { text: 'Fulfill Request', value: 'actions', sortable: false },
        ],
        itemsPendingFulfilment: [],
        productsToShow: [],
        competitorsToShow: [],
        loading: false,
        editedItem: {
          customerName: '',
          productName: '',
          budget: '',
          fulfilledPrice: null,
          dealJustification: '',
          deliveryDate: '',
          remarks: '',
          tenderDate: ''
        },
      }
    },
    watch: {
      options: {
        handler() {
          this.getFulfillerItem()
        },
        deep: true,
      },
      dialog(val) {
        val || this.close()
      }
    },
    created() {
      this.getFulfillerItem();
    },

    methods: {
      today() {
        const date = new Date();
        const year = date.getFullYear();
        const month = ('0' + (date.getMonth() + 1)).slice(-2);
        const day = ('0' + date.getDate()).slice(-2);
        const hours = ('0' + date.getHours()).slice(-2);
        const minutes = ('0' + date.getMinutes()).slice(-2);

        return `${year}-${month}-${day}T${hours}:${minutes}`;
      },
      getFulfillerItem() {
        this.loading = true
        this.$axios.get(`${this.$config.restUrl}/api/request/getpendingfulfilleritem`).then(result => {
          this.itemsPendingFulfilment = [];
          result.data.data.forEach(item => {
            item.requestProductsModel.forEach(product => {
              const newItem = {
                ...item,
                ...product,
                productName: product.productName,
                budget: product.budget,
                quantity: product.quantity,
                competitors: [],
              };
              item.competitorInformationModel.forEach(comp => {
                const c = {
                  model: comp.model,
                  brand: comp.brand,
                  price: comp.price
                }
                newItem.competitors.push(c); 
              });
              this.itemsPendingFulfilment.push(newItem);
              const p = {
                quantity: product.quantity,
                budget: product.budget,
                productName: product.productName,
                tenderDate: product.tenderDate,
                remarks: product.remarks
              };
              this.productsToShow.push(p);
            });
          });
          this.loading = false
        })
      },
      editItem(item) {
        this.editedItem = { ...item, deliveryDate: this.today() };
        this.competitorsToShow = [...item.competitors]; 
        this.dialog = true;
      },
      fulfillRequest() {
        if (this.editedItem && this.editedItem.fulfilledPrice !== null && this.editedItem.fulfilledPrice > 0) {
          Swal.fire({
            title: 'Fulfill Request',
            text: 'Are you sure you want to fulfill this request?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Fulfill',
          }).then((result) => {
            if (result.isConfirmed) {
              this.$axios.post(`${this.$config.restUrl}/api/request/fulfillrequest?requestId=${this.editedItem.requestId}&productId=${this.editedItem.productId}&fulfilledPrice=${this.editedItem.fulfilledPrice}&deliveryDate=${this.editedItem.deliveryDate}&remarks=${this.editedItem.remarks}`)
                .then(response => {
                  this.closeDialog();
                  Swal.fire('Fulfilled!', 'Request has been fulfilled.', 'success');
                }).catch(error => {
                  console.log('error', error);
                  Swal.fire('Error', 'Failed to fulfill request', 'error');
                });
            }
          });
        }
        else {
          this.$swal('Error', 'Please fill in all fields!', 'error');
        }
      },
      close() {
        this.dialog = false
        this.$nextTick(() => {
          this.editedItem = Object.assign({}, this.defaultItem)
          this.editedIndex = -1
        })
      },
      closeDialog() {
        this.dialog = false;
        this.editedItem = {
          productName: '',
          budget: '',
          fulfilledPrice: ''
        };
      },
      closeDelete() {
        this.dialogDelete = false
        this.$nextTick(() => {
          this.editedItem = Object.assign({}, this.defaultItem)
          this.editedIndex = -1
        })
      }
    },
  }
</script>

<style scoped>
  .mini-table {
    width: 100%;
    margin-top: 2rem;
    border-collapse: collapse;
  }

    .mini-table th, .mini-table td {
      border: 1px solid #ddd;
      padding: 8px;
      text-align: left;
    }

    .mini-table tr:nth-child(even) {
      background-color: #f2f2f2;
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

  .readonly-field {
    background-color: #ddd;
  }
</style>
