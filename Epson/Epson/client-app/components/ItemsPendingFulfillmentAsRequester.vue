<template>
  <v-data-table :headers="headers"
                :items="requests"
                :options.sync="options"
                :items-per-page="5"
                :loading="loading"
                class="elevation-1">
    <template v-slot:top>
      <v-toolbar flat>
        <v-toolbar-title>Items Pending Fulfillment As Requester</v-toolbar-title>
        <v-divider class="mx-4" inset vertical></v-divider>
        <v-spacer></v-spacer>
        <v-dialog v-model="dialog" max-width="500px">
          <v-card>
            <v-card-title>
              <span class="text-h5">{{ formTitle }}</span>
            </v-card-title>
            <v-card-text>
              <label>Request</label>
              <div class="form-group">
                <label>CreatedOnUTC</label>
                <input v-model="editedItem.createdOnUTC" class="border-input" label="Date" disabled></input>
              </div>
              <div class="form-group">
                <label>Priority</label>
                <input v-model="editedItem.priority" class="border-input" label="Priority" disabled></input>
              </div>
              <div class="form-group">
                <label>Total Price</label>
                <input v-model="editedItem.totalPrice" class="border-input" label="Price" disabled></input>
              </div>
              <div class="form-group">
                <label>Total Budget</label>
                <input v-model="editedItem.totalBudget" class="border-input" label="Budget" disabled></input>
              </div>
            </v-card-text>
          </v-card>
        </v-dialog>
      </v-toolbar>
    </template>
  </v-data-table>

</template>

<script>
  import moment from 'moment';

  export default {
    name: 'ItemsPendingRequesterAction',
    data() {
      return {
        error: null,
        dialog: false,
        dialogDelete: false,
        headers: [
          {
            text: 'ID',
            align: ' d-none',
            value: 'id',
          },
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
          }
        ],
        options: {},
        requests: [],
        loading: true,
        editedIndex: -1,
        editedItem: {
          id: 0,
          name: '',
          price: 0,
        },
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
          this.getPendingFulfillmentAsRequester()
        },
        deep: true,
      },
      dialog(val) {
        val || this.close()
      },
    },
    methods: {
      getPendingFulfillmentAsRequester() {
        this.loading = true
        this.$axios.get(`${this.$config.restUrl}/api/request/getpendingfulfillmentasrequester`).then(result => {
          for (const requests in result.data.data) {
            result.data.data[requests].createdOnUTC = moment(this.editedItem.createdOnUTC).format('MMMM Do YYYY');
          }
          this.requests = result.data.data
          this.loading = false
          console.log("requests", this.requests);
        })
      }
    },
  }
</script>
<style>
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
</style>
