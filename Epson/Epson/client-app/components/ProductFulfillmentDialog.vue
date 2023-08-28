<template>
  <v-dialog v-model="localDialogProductFulfillment" max-width="800px">
    <v-card>
      <v-card-title>
        <span class="text-h5">RESPONDENT - Product Managers</span>
      </v-card-title>
      <v-card-text>
        <div class="form-group">
          <label>Requested By</label>
          <input v-model="localEditedItem.createdBy" class="border-input readonly-field" label="Requested By" readonly></input>
        </div>
        <div class="form-group">
          <label>Product</label>
          <input v-model="localEditedItem.productName" class="border-input readonly-field" label="Product" readonly></input>
        </div>
        <div class="form-group">
          <label>Dealer Price</label>
          <input v-model="localEditedItem.dealerPrice" class="border-input readonly-field" label="Dealer Price" readonly></input>
        </div>
        <div class="form-group">
          <label>End User Price</label>
          <input v-model="localEditedItem.endUserPrice" class="border-input readonly-field" label="End User Price" readonly></input>
        </div>
        <div class="form-group">
          <label>Quantity</label>
          <input v-model="localEditedItem.quantity" class="border-input readonly-field" label="Quantity" readonly></input>
        </div>
        <div class="form-group">
          <label>Remarks</label>
          <input v-model="localEditedItem.remarks" class="border-input" label="Remarks"></input>
        </div>
        <div class="form-group">
          <label>Dealer Price</label>
          <input v-model="localEditedItem.fulfilledPrice" type="number" class="border-input" label="Approved Price" required></input>
        </div>

        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="blue darken-1" text @click="rejectRequest">Reject</v-btn>
          <v-btn color="blue darken-1" text @click="fulfillRequest">Approve</v-btn>
        </v-card-actions>
      </v-card-text>
    </v-card>
  </v-dialog>
</template>

<script>
  import Swal from 'sweetalert2';
  export default {
    name: 'DialogComponent',
    props: {
      dialogProductFulfillment: {
        type: Boolean,
        default: false
      },
      editedItem: Object,
      competitorsToShow: Array,
    },
    data() {
      return {
        localDialogProductFulfillment: this.dialogProductFulfillment,
        localEditedItem: {},
      };
    },
    watch: {
      dialogProductFulfillment(newVal, oldVal) {
        this.localDialogProductFulfillment = newVal;
      },
      localDialogProductFulfillment(newVal, oldVal) {
        this.$emit('update:dialogProductFulfillment', newVal);
      },
      editedItem: {
        handler(newValue) {
          this.localEditedItem = { ...newValue };
        },
        immediate: true,
      },
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
      fulfillRequest() {
        if (this.localEditedItem && this.localEditedItem.fulfilledPrice !== null && this.localEditedItem.fulfilledPrice > 0) {
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
              this.$axios.post(`${this.$config.restUrl}/api/request/fulfillrequest?id=${this.localEditedItem.id}&productId=${this.localEditedItem.productId}&fulfilledPrice=${this.localEditedItem.fulfilledPrice}&remarks=${this.localEditedItem.remarks}`)
                .then(response => {
                  this.close(); 
                  Swal.fire('Fulfilled!', 'Request has been fulfilled.', 'success')
                    .then(() => {
                      this.$router.push('/productDashboard');
                    });
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
        if (this.localEditedItem.remarks !== "" && this.localEditedItem.remarks !== null) {
          Swal.fire({
            title: 'Reject Request?',
            showCancelButton: true,
            confirmButtonText: 'Reject Request',
          }).then((result) => {
            if (result.isConfirmed) {
              this.$axios.post(`${this.$config.restUrl}/api/request/rejectrequestproduct?requestProductId=${this.localEditedItem.id}&remarks=${this.localEditedItem.remarks}`)
                .then(response => {
                  this.close();
                  Swal.fire('Rejected!', 'Request has been rejected.', 'success')
                    .then(() => {
                      this.$router.push('/productDashboard');
                    });
                }).catch(error => {
                  console.log('error', error);
                  Swal.fire('Error', 'Failed to reject request', 'error');
                });
            }
          })
        } else {
          this.$swal('Error', 'Remarks must not be empty!', 'error');
        }
      },
      close() {
        this.localDialogProductFulfillment = false;
        this.$emit('closeDialog');
      },
    }
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
