<template>
  <v-app>
    <div class="table-container">
      <v-data-table :headers="headers"
                    :items="requests"
                    :options.sync="options"
                    :items-per-page="5"
                    :loading="loading">
        <template v-slot:top>
          <v-toolbar flat>
            <v-toolbar-title>Requests</v-toolbar-title>
            <v-divider class="mx-4" inset vertical></v-divider>
            <v-spacer></v-spacer>
          </v-toolbar>
        </template>
        <template v-slot:item.actions="{ item }">
          <v-icon small class="mr-2" @click="editItem(item)">
            mdi-pencil
          </v-icon>
        </template>
      </v-data-table>

      <v-dialog v-model="dialog" max-width="500px">
        <v-card>
          <v-card-title>
            <span class="text-h5">{{ formTitle }}</span>
          </v-card-title>
          <v-card-text>
            <!-- Dialog content goes here -->
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
          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="blue darken-1" text @click="close">Approve</v-btn>
            <v-btn color="blue darken-1" text @click="save">Reject</v-btn>
          </v-card-actions>
        </v-card>
      </v-dialog>
    </div>
  </v-app>
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
            text: 'Created Time',
            align: 'start',
            value: 'createdOnUTC',
          },
          {
            text: 'Approval State',
            align: 'start',
            value: 'approvalStateStr',
          },
          {
            text: 'Total Budget',
            align: 'start',
            value: 'totalBudget',
          },
          { text: 'Approve Request', value: 'actions', sortable: false },
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
          this.getDataFromApi()
        },
        deep: true,
      },
      dialog(val) {
        val || this.close()
      },
    },
    methods: {
      getDataFromApi() {
        this.loading = true
        this.$axios.get(`${this.$config.restUrl}/api/request/getpendingfulfillmentasrequester`).then(result => {
          for (const requests in result.data.data) {
            result.data.data[requests].createdOnUTC = moment(this.editedItem.createdOnUTC).format('MMMM Do YYYY');
          }
          this.requests = result.data.data
          this.loading = false
        })
      },
      editItem(item) {
        this.editedIndex = this.requests.indexOf(item)
        this.editedItem = { ...item };
        this.dialog = true
      },

      async close() {
        const vm = this;
        try {
          await this.$axios.post(`${this.$config.restUrl}/api/request/approverequest?id=${this.editedItem.id}`).then(response => {
            this.getDataFromApi();
            this.dialog = false;
            this.close();
          }).catch(err => {
            console.log(err);
            console.log(err.response);
            vm.$swal('Failed to update', err.response.data.message, 'error');
          })
        } catch (err) {
          console.log(err);
        }

      },

      async save() {
        console.log(this.editedItem.id);
        const vm = this;
        try {
          await this.$axios.post(`${this.$config.restUrl}/api/request/approverequest?id=${this.editedItem.id}`).then(response => {
            this.getDataFromApi();
            this.close();
            this.dialog = false;
          }).catch(err => {
            console.log(err);
            console.log(err.response);
            vm.$swal('Failed to update', err.response.data.message, 'error');
          })
        } catch (err) {
          console.log(err);
        }
      },

    },
  }
</script>
<style>
  .table-container {
    width: 800px;
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
</style>
