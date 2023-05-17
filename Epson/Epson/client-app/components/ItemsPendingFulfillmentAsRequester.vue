<template>
  <v-data-table :headers="headers"
                :items="requests"
                :options.sync="options"
                :items-per-page="5"
                :loading="loading"
                class="elevation-1">
    <template v-slot:top>
      <v-toolbar flat>
        <v-toolbar-title>Pending</v-toolbar-title>
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
      <v-dialog v-model="quotationDialog" max-width="500px">
        <v-card class="mx-auto" width="800">
          <v-card-title class="py-4">
            <span class="text-h5">Edit Pricing Request</span>
          </v-card-title>
          <v-card-text>
            <div class="form-group">
              <label>Customer Name</label>
              <input type="text" v-model="customerName" class="border-input" typeof="text">
            </div>
            <label>Product Categories</label>
            <div v-for="(category, index) in categories" :key="'C'+index">
              <div class="blue-checkbox">
                <input type="checkbox" v-model="selectedCategories" :value="category" @change="checkboxChanged(category)">
                <label class="category-name">{{ category.name }}</label>
              </div>
            </div>
            <div v-for="category in selectedCategories" :key="category.id">
              <div class="form-group">
                <label>{{ category.name }}</label>
                <select v-model="selectedProducts[category.id]" required class="border-input">
                  <option v-for="option in options[category.id]" :value="option.id" :key="option.id">{{ option.name }}</option>
                </select>
              </div>

              <div class="form-group">
                <label>Quantity</label>
                <input v-model="quantity[category.id]" class="border-input" type="text">
              </div>
              <div class="form-group">
                <label>Budget</label>
                <input v-model="budget[category.id]" class="border-input" type="text">
              </div>
              <div class="form-group">
                <label>Priority</label>
                <select v-model="priority.value" class="border-input">
                  <option v-for="option in priority.options" :value="option.value" :key="option.value">
                    {{ option.label }}
                  </option>
                </select>
              </div>
              <div class="form-group">
                <label>Deal Justification</label>
                <textarea v-model="dealJustification" class="border-input"></textarea>
              </div>
              <div class="form-group">
                <label>Deadline</label>
                <input type="datetime-local" v-model="deadline" class="border-input">
              </div>
            </div>
            <button class="dialog-button" type="submit" @click="editQuotation">Edit Quotation</button>
          </v-card-text>
        </v-card>
      </v-dialog>
    </template>
    <template v-slot:item.actions="{ item }">
      <v-icon small
              class="mr-2"
              v-if="item.approvalState !== 40"
              @click="confirmAmmendQuotation(item)">
        mdi-check
      </v-icon>
      <v-icon small
              @click="dialogQuotation(item)"
              v-if="item.approvalState === 40">
        mdi-pencil
      </v-icon>
    </template>
  </v-data-table>

</template>

<script>
  import moment from 'moment';
  import Swal from 'sweetalert2';

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
          },
          { text: 'Actions', value: 'actions', sortable: false },
        ],
        options: {},
        requests: [],
        loading: true,
        quotationDialog: false,
        editedIndex: -1,
        editedItem: {
          id: 0,
          name: '',
          price: 0,
        },
        requestId: 0,
        
        selectedCategories: [],
        categories: [],
        selectedProducts: {},
        quantity: {},
        budget: {},
        priority: {
          value: 1,
          options: [
            { value: 1, label: 'High' },
            { value: 2, label: 'Medium' },
            { value: 3, label: 'Low' }
          ]
        },
        customerName: '',
        dealJustification: '',
        deadline: '',
      }
    },
    created() {
      this.categories.forEach(category => {
        this.options[category.id] = [];
      });
      this.fetchCategories();
    },
    computed: {
      formTitle() {
        return 'Request'
      },

    },
    watch: {
      options: {
        handler() {
          this.getPendingFulfillmentAsRequester()
        },
        deep: true,
      },
      quotationDialog(newVal, oldVal) {
        if (newVal === false && oldVal === true) {
          this.resetData();
        }
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
      },
      checkboxChanged(selectedCategory) {
        if (selectedCategory) {
          const categoryId = selectedCategory.id;
          if (this.selectedCategories.includes(selectedCategory)) {
            if (this.selectedProducts[categoryId] === undefined) {
              this.selectedProducts[categoryId] = '';
              this.quantity[categoryId] = null;
              this.budget[categoryId] = null;
            }
          } else {
            this.$delete(this.selectedProducts, categoryId);
            this.$delete(this.quantity, categoryId);
            this.$delete(this.budget, categoryId);
            this.$delete(this.options, categoryId);
          }
        }
      },
      confirmAmmendQuotation(item) {
        Swal.fire({
          title: 'Confirmation',
          text: 'Are you sure you want to amend the quotation?',
          icon: 'warning',
          showCancelButton: true,
          confirmButtonColor: '#3085d6',
          cancelButtonColor: '#d33',
          confirmButtonText: 'Yes',
          cancelButtonText: 'No'
        }).then((result) => {
          if (result.isConfirmed) {
            this.$axios.post(`${this.$config.restUrl}/api/request/setrequesttoamendquotation?requestId=${item.id}`).then(result => {
              for (const requests in result.data.data) {
                result.data.data[requests].createdOnUTC = moment(this.editedItem.createdOnUTC).format('MMMM Do YYYY');
              }
            });
          }
        });
      },
      async dialogQuotation(item) {
        console.log("item", item);
        this.requestId = item.id;
        this.quotationDialog = true;
        for (const productModel of item.requestProductsModel) {
          const category = this.categories.find((category) => category.id === productModel.productCategory.categoryId);
          if (category) {
            this.selectedCategories.push(category);
            await this.fetchProductsForCategory(category);
            this.selectedProducts[category.id] = productModel.productId;
            this.quantity[category.id] = productModel.quantity;
            this.budget[category.id] = productModel.budget;
          }
        }
        this.customerName = item.customerName;
        this.dealJustification = item.dealJustification;
        this.deadline = item.deadline;
        this.priority.value = item.priority;
      },
      resetData() {
        this.requestId = 0;
        this.selectedCategories = [];
        this.selectedProducts = {};
        this.quantity = {};
        this.budget = {};
        this.priority = 0;
      },
      async fetchProductsForCategory(category) {
        try {
          const response = await this.$axios.get(`${this.$config.restUrl}/api/product/getproductbycategory`, { params: { categoryId: category.id } });
          this.$set(this.options, category.id, response.data.data);
        } catch (error) {
          console.error(error);
        }
      },
      async editQuotation() {
        const quotationData = {
          ApprovalState: 20,
          Priority: this.priority,
          requestProducts: [],
        };
        for (const categoryId in this.selectedProducts) {
          const idProduct = this.selectedProducts[categoryId];
          if (idProduct !== '' && idProduct !== null) {
            const product = {
              productId: idProduct,
              fulfillerId: "string",
              quantity: this.quantity[categoryId],
              budget: this.budget[categoryId],
            };
            quotationData.requestProducts.push(product);
          }
        }
        try {
          const vm = this;
          await this.$axios.post(`${this.$config.restUrl}/api/request/editrequest`, {
            data: {
              id: this.requestId,
              segment: "string",
              approvalState: 10,
              priority: this.priority.value,
              RequestProducts: quotationData.requestProducts
            }
          }).then(response => {
            this.$swal('Request updated');
            this.quotationDialog = false;
            this.$router.go();

          }).catch(err => {
            console.log(err);
            vm.$swal('Failed to update request', err.response.data.message, 'error');
          })
        } catch (error) {
          console.log(error);
        }
      },
      async fetchCategories() {
        try {
          await this.$axios.get(`${this.$config.restUrl}/api/category/getcategories`).then(response => {
            this.categories = response.data.data;

          });

        } catch (error) {
          console.error(error);
        }
      },
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

  .dialog-button {
    padding: 0.5rem 1rem;
    background-color: #003399 !important;
    color: #fff !important;
    border: none;
    cursor: pointer;
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
