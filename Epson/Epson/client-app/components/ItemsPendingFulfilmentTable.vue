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

        <ProductFulfillmentDialog :editedItem="editedItem"
                                  :competitorsToShow="competitorsToShow"
                                  :dialog.sync="dialog"
                                  @close="close"></ProductFulfillmentDialog>
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
  import ProductFulfillmentDialog from '~/components/ProductFulfillmentDialog.vue';
  export default {
    name: 'ItemsPendingFulfilmentTable',
    components: {
      ProductFulfillmentDialog
    },
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
      rejectRequest() {
        Swal.fire({
          title: 'Reject Request?',
          showCancelButton: true,
          confirmButtonText: 'Reject Request',
        }).then((result) => {
          if (result.isConfirmed) {
            this.$axios.post(`${this.$config.restUrl}/api/request/rejectrequestproduct?requestProductId=${this.editedItem.id}&remarks=${this.editedItem.remarks}`)
              .then(response => {
                this.closeDialog();
                Swal.fire('Rejected!', 'Request has been rejected.', 'success');
              }).catch(error => {
                console.log('error', error);
                Swal.fire('Error', 'Failed to reject request', 'error');
              });
          }
        })
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
