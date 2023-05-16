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

        <v-dialog v-model="dialog" max-width="500px">
          <v-card>
            <v-card-title>
              <span class="text-h5">{{ formTitle }}</span>
            </v-card-title>
            <v-card-text>
              <div class="form-group">
                <label>Product</label>
                <input v-model="editedItem.productName" class="border-input" label="Product" readonly></input>
              </div>
              <div class="form-group">
                <label>Budget</label>
                <input v-model="editedItem.budget" class="border-input" label="Budget" readonly></input>
              </div>
              <div class="form-group">
                <label>Quantity</label>
                <input v-model="editedItem.quantity" class="border-input" label="Quantity" readonly></input>
              </div>
              <div class="form-group">
                <label>Approved Price</label>
                <input v-model="editedItem.fulfilledPrice" class="border-input" label="Approved Price"></input>
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

    <template v-slot:no-data>
      <v-btn color="primary" @click="fetchPendingItems">
        Reset
      </v-btn>
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
          { text: 'Product', value: 'productName' },
          { text: 'Budget', value: 'budget' },
          { text: 'Quantity', value: 'quantity' },
          { text: 'Fulfill Request', value: 'actions', sortable: false },
        ],
        itemsPendingFulfilment: [],
        loading: false,
        editedItem: {
          productName: '',
          budget: '',
          fulfilledPrice: null
        },
      }
    },
    computed: {
      formTitle() {
        return this.editedIndex === -1 ? 'New Item' : 'Edit Item'
      },
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
      getFulfillerItem() {
        this.loading = true
        this.$axios.get(`${this.$config.restUrl}/api/request/getpendingfulfilleritem`).then(result => {
          this.itemsPendingFulfilment = [];
          result.data.data.forEach(item => {
            item.requestProductsModel.forEach(product => {
              this.itemsPendingFulfilment.push({
                ...item,
                ...product,
                productName: product.productName,
                budget: product.budget,
                quantity: product.quantity,
              });
            });
          });
          this.loading = false
        })
      },
      editItem(item) {
        this.editedItem = { ...item };
        this.dialog = true;
      },
      fulfillRequest() {
        if (this.editedItem && this.editedItem.fulfilledPrice !== null) {
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
              this.$axios.post(`${this.$config.restUrl}/api/request/fulfillrequest?requestId=${this.editedItem.requestId}&productId=${this.editedItem.productId}&fulfilledPrice=${this.editedItem.fulfilledPrice}`)
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
</style>
