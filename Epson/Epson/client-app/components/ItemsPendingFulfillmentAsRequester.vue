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
      <v-dialog v-model="quotationDialog" max-width="800px">
        <v-card class="mx-auto" width="800">
          <v-card-title class="py-4">
            <span class="text-h5">Edit Pricing Request</span>
          </v-card-title>
          <v-card-text>
            <div class="form-group">
              <label>Customer Name</label>
              <input type="text" v-model="customerName" class="border-input" typeof="text">
            </div>
            <v-dialog v-model="dialogProduct" max-width="500px">
              <v-card>
                <v-card-title>
                  <span class="headline">Add Product</span>
                </v-card-title>
                <v-card-text>
                  <div class="form-group">
                    <label>Product Categories</label>
                    <select v-model="product.category" @change="updateProductOptions">
                      <option v-for="category in categories" :value="category" :key="category.id">{{ category.name }}</option>
                    </select>
                    <label>Product</label>
                    <select v-model="product.productId">
                      <option v-for="option in productOptions" :value="option.id" :key="option.id">{{ option.name }}</option>
                    </select>
                    <label>Quantity</label>
                    <input v-model="product.quantity" class="border-input" type="number" min="1">
                    <label>Budget</label>
                    <input v-model="product.budget" class="border-input" type="number" min="1">
                    <label>Remarks</label>
                    <input type="text" v-model="product.remarks" class="border-input">
                    <label>Tender Date</label>
                    <input type="datetime-local" v-model="product.tenderDate" class="border-input">
                  </div>
                </v-card-text>
                <v-card-actions>
                  <v-spacer></v-spacer>
                  <v-btn color="primary" @click="addProductRow">Add</v-btn>
                  <v-btn color="secondary" @click="dialog = false">Cancel</v-btn>
                </v-card-actions>
              </v-card>
            </v-dialog>
            <div class="table-actions mb-4">
              <v-btn class="add-product-btn" fab small color="primary" @click="dialogProduct = true">
                <v-icon>mdi-plus</v-icon>
              </v-btn>
            </div>
            <label>Products</label>
            <table class="mb-5 mt-2 mini-table">
              <thead>
                <tr>
                  <th>Category</th>
                  <th>Product</th>
                  <th>Quantity</th>
                  <th>Budget</th>
                  <th>Remarks</th>
                  <th>Tender Date</th>
                  <th>Action</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="(product, index) in productsToShow" :key="index">
                  <td>{{ product.category ? product.category.name : 'N/A' }}</td>
                  <td>{{ product.productId ? findProductName(product.productId) : product.productName }}</td>
                  <td>{{ product.quantity || 'N/A' }}</td>
                  <td>{{ product.budget || 'N/A' }}</td>
                  <td>{{ product.remarks || 'N/A' }}</td>
                  <td>{{ product.tenderDate || 'N/A' }}</td>
                  <td>
                    <v-btn small color="error" @click="removeProduct(index)">
                      <v-icon>mdi-delete</v-icon>
                    </v-btn>
                  </td>
                </tr>
              </tbody>
            </table>
            <v-dialog v-model="dialogCompetitor" max-width="500px">
              <v-card>
                <v-card-title>
                  <span class="headline">Add Competitor Information</span>
                </v-card-title>
                <v-card-text>
                  <div class="form-group">
                    <label>Model</label>
                    <input type="text" v-model="competitor.model" class="border-input">
                    <label>Brand</label>
                    <input type="text" v-model="competitor.brand" class="border-input">
                    <label>Price</label>
                    <input v-model="competitor.price" class="border-input" type="number" min="1">
                  </div>
                </v-card-text>
                <v-card-actions>
                  <v-spacer></v-spacer>
                  <v-btn color="primary" @click="addCompetitorInformationRow">Add</v-btn>
                  <v-btn color="secondary" @click="dialog = false">Cancel</v-btn>
                </v-card-actions>
              </v-card>
            </v-dialog>
            <div class="table-actions mb-4">
              <v-btn class="add-competitor-btn" fab small color="primary" @click="dialogCompetitor = true">
                <v-icon>mdi-plus</v-icon>
              </v-btn>
            </div>
            <label>Competitor Information</label>
            <table class="mb-5 mt-2 mini-table">
              <thead>
                <tr>
                  <th>Model</th>
                  <th>Brand</th>
                  <th>Price</th>
                  <th>Action</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="(competitor, index) in competitorsToShow" :key="index">
                  <td>{{ competitor.model }}</td>
                  <td>{{ competitor.brand }}</td>
                  <td>{{ competitor.price }}</td>
                  <td>
                    <v-btn small color="error" @click="removeCompetitorInformation(index)">
                      <v-icon>mdi-delete</v-icon>
                    </v-btn>
                  </td>
                </tr>
              </tbody>
            </table>
            <div class="form-group">
              <label>Priority</label>
              <select v-model="priority.value" class="border-input">
                <option v-for="option in priority.options" :value="option.value" :key="option.value">
                  {{ option.label }}
                </option>
              </select>
            </div>
            <div class="form-group">
              <label>Requirements</label>
              <textarea v-model="dealJustification" class="border-input"></textarea>
            </div>
            <div class="form-group">
              <label>Deadline</label>
              <input type="datetime-local" v-model="deadline" class="border-input">
            </div>
            <button class="dialog-button" type="submit" @click="editQuotation">Edit Quotation</button>
          </v-card-text>
        </v-card>
      </v-dialog>
    </template>
    <template v-slot:item.actions="{ item }">
      <v-icon small
              class="mr-2"
              v-if="item.approvalState !== ApprovalStateEnum.AmendQuotation"
              @click="confirmAmmendQuotation(item)">
        mdi-check
      </v-icon>
      <v-icon small
              @click="cancelRequest(item)">
        mdi-cancel
      </v-icon>
      <v-btn @click="viewRequest(item)">View</v-btn>
    </template>
  </v-data-table>

</template>

<script>
  import moment from 'moment';
  import Swal from 'sweetalert2';
  import { ApprovalStateEnum } from '~/script/approvalStateEnum.js';

  export default {
    name: 'ItemsPendingRequesterAction',
    data() {
      return {
        error: null,
        dialog: false,
        dialogProduct: false,
        dialogCompetitor: false,
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
        loading: false,
        quotationDialog: false,
        editedIndex: -1,
        editedItem: {
          id: 0,
          name: '',
          price: 0,
        },
        requestId: 0,
        productOptions: [],
        productsToShow: [],
        product: { category: null, productId: null, quantity: null, budget: null, remarks: null, tenderDate: null },
        products: [],
        competitorsToShow: [],
        competitor: { model: null, brand: null, price: null },
        competitors: [],
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
        ApprovalStateEnum
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
      viewRequest(request) {
        let queryParameters = { request: JSON.stringify(request) };

        if (request.approvalState === ApprovalStateEnum.AmendQuotation) {
          queryParameters = { ...queryParameters, editable: true };
        } else {
          queryParameters = { ...queryParameters, view: true };
        }

        this.$router.push({
          path: '/createquotation',
          query: queryParameters
        });
      },
      addCompetitorInformationRow() {
        if (this.competitor.brand && this.competitor.model && this.competitor.price) {
          const newCompetitor = { ...this.competitor };
          this.competitors.push(newCompetitor);
          this.showAddedCompetitors(newCompetitor);
          this.competitor.brand = null;
          this.competitor.model = null;
          this.competitor.price = null;
          this.dialogCompetitor = false;
        } else {
          this.$swal('Error', 'Please fill out all product fields', 'error');
        }
      },
      removeCompetitorInformation(index) {
        this.competitorsToShow.splice(index, 1);
      },
      showAddedCompetitors(newCompetitor) {
        this.competitorsToShow.push(newCompetitor);
      },
      addProductRow() {
        if (this.product.category && this.product.productId && this.product.quantity && this.product.budget) {
          const newProduct = { ...this.product };
          this.products.push(newProduct);
          this.showAddedProducts(newProduct);
          this.product.category = null;
          this.product.productId = null;
          this.product.quantity = null;
          this.product.budget = null;
          this.product.remarks = null;
          this.product.tenderDate = null;
          this.productOptions = [];
          this.dialogProduct = false;
        } else {
          this.$swal('Error', 'Please fill out all product fields', 'error');
        }
      },
      removeProduct(index) {
        this.productsToShow.splice(index, 1);
      },
      showAddedProducts(newProduct) {
        this.productsToShow.push(newProduct);
      },
      findProductName(productId) {
        for (const category of this.categories) {
          for (const product of category.products) {
            if (product.id === productId) {
              return product.name;
            }
          }
        }
        return 'N/A';
      },
      async updateProductOptions() {
        const category = this.product.category;

        if (category) {
          try {
            const response = await this.$axios.get(`${this.$config.restUrl}/api/product/getproductbycategory`, { params: { categoryId: category.id } });
            this.productOptions = response.data.data;
          } catch (error) {
            console.error(error);
          }
        }
      },
      getPendingFulfillmentAsRequester() {
        this.loading = true
        this.$axios.get(`${this.$config.restUrl}/api/request/getpendingfulfillmentasrequester`).then(result => {
          for (const requests in result.data.data) {
            result.data.data[requests].createdOnUTC = moment(this.editedItem.createdOnUTC).format('DD MMM YY HH:mm');
          }
          this.requests = result.data.data
          this.loading = false
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
            this.$axios.post(`${this.$config.restUrl}/api/request/setrequesttoamendquotation?requestId=${item.id}`)
            .then(response => {
              Swal.fire('Amended!', 'Request is in amend stage.', 'success')
                .then(() => {
                this.$router.push('/salesDashboard');
              });
            }).catch(error => {
              console.log('error', error);
              Swal.fire('Error', 'Failed to amend request', 'error');
            });
          }
        });
      },
      cancelRequest(item) {
        Swal.fire({
          title: 'Cancel Request?',
          input: 'textarea', 
          inputPlaceholder: 'Remark',
          showCancelButton: true,
          confirmButtonText: 'Cancel Request',
        }).then((result) => {
          if (result.isConfirmed) {
            this.$axios.post(`${this.$config.restUrl}/api/request/cancelrequest?requestId=${item.id}&remarks=${result.value}`)
            .then(response => {
              Swal.fire('Cancelled!', 'Request has been cancelled.', 'success')
                .then(() => {
                  this.$router.push('/salesDashboard');
                });
            }).catch(error => {
              console.log('error', error);
              Swal.fire('Error', 'Failed to cancel request', 'error');
            });
          }
        })
      },
      async dialogQuotation(item) {
        this.requestId = item.id;
        this.quotationDialog = true;
        for (const productModel of item.requestProductsModel) {
          const categoryFound = this.categories.find((categoryFound) => categoryFound.id === productModel.productCategory.categoryId);
          if (categoryFound) {
            this.selectedCategories.push(categoryFound);
            await this.fetchProductsForCategory(categoryFound);
            const p = {
              category: categoryFound,
              productId: productModel.productId,
              quantity: productModel.quantity,
              distyPrice: productModel.distyPrice,
              dealerPrice: productModel.dealerPrice,
              endUserPrice: productModel.endUserPrice,
              productName: productModel.productName,
              tenderDate: productModel.tenderDate,
              remarks: productModel.remarks
            };
            this.productsToShow.push(p);
          }
        }
        for (const competitorModel of item.competitorInformationModel) {
          const c = {
            model: competitorModel.model,
            brand: competitorModel.brand,
            price: competitorModel.price
          };
          this.competitorsToShow.push(c);
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
        this.productsToShow = [];
        this.competitorsToShow = [];
        this.quantity = {};
        this.budget = {};
        this.priority.value = 0;
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
          competitorInformations: []
        };

        for (const product in this.productsToShow) {
          const productToInsert = {
            productId: this.productsToShow[product].productId,
            quantity: this.productsToShow[product].quantity,
            distyPrice: this.productsToShow[product].distyPrice,
            dealerPrice: this.productsToShow[product].dealerPrice,
            endUserPrice: this.productsToShow[product].endUserPrice,
            tenderDate: this.productsToShow[product].tenderDate,
            remarks: this.productsToShow[product].remarks,
          };
          quotationData.requestProducts.push(productToInsert);
        }
        for (const competitor in this.competitorsToShow) {
          const competitorToInsert = {
            model: this.competitorsToShow[competitor].model,
            brand: this.competitorsToShow[competitor].brand,
            price: this.competitorsToShow[competitor].price
          };
          quotationData.competitorInformations.push(competitorToInsert);
        }

        try {
          await this.$axios.post(`${this.$config.restUrl}/api/request/editrequest`, {
            data: {
              id: this.requestId,
              segment: "string",
              approvalState: 10,
              priority: this.priority.value,
              RequestProducts: quotationData.requestProducts,
              CompetitorInformations: quotationData.competitorInformations,
              customerName: this.customerName,
              dealJustification: this.dealJustification,
              deadline: this.deadline,
              comments: '',
            }
          });

          Swal.fire({
            title: 'Request updated',
            text: 'Do you want to reload the page to see the updated request?',
            icon: 'success',
            showCancelButton: true,
            confirmButtonText: 'Yes, reload',
            cancelButtonText: 'No, later'
          }).then((result) => {
            if (result.isConfirmed) {
              location.reload(); 
            }
          });
        } catch (error) {
          Swal.fire({
            title: 'Failed to update request',
            text: error.response.data.message,
            icon: 'error'
          });
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
