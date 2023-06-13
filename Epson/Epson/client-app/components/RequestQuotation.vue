<template>
  <div class="create-quotation-container">
    <h1>Pricing Request</h1>
    <v-card class="mx-auto" width="800">
      <v-card-text>
        <div class="form-group">
          <label>Customer Name</label>
          <input type="text" v-model="customerName" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode">
        </div>
        <v-dialog v-model="dialog" max-width="500px">
          <v-card>
            <v-card-title>
              <span class="headline">Add Product</span>
            </v-card-title>
            <v-card-text>
              <div class="form-group">
                <label>Product Categories</label>
                <select v-model="product.category" @change="updateProductOptions" :class="{'readonly-field': isViewMode}" :readonly="isViewMode">
                  <option v-for="category in categories" :value="category" :key="category.id">{{ category.name }}</option>
                </select>
                <label>Product</label>
                <select v-model="product.productId" :class="{'readonly-field': isViewMode}" :readonly="isViewMode">
                  <option v-for="option in productOptions" :value="option.id" :key="option.id">{{ option.name }}</option>
                </select>
                <label>Quantity</label>
                <input v-model="product.quantity" class="border-input" type="number" min="1" :class="{'readonly-field': isViewMode}" :readonly="isViewMode">
                <label>Budget</label>
                <input v-model="product.budget" class="border-input" type="number" min="1" :class="{'readonly-field': isViewMode}" :readonly="isViewMode">
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
          <v-btn v-if="!isViewMode" class="add-product-btn" fab small color="primary" @click="dialog = true">
            <v-icon>mdi-plus</v-icon>
          </v-btn>
        </div>
        <table class="mb-5">
          <thead>
            <tr>
              <th>Category</th>
              <th>Product</th>
              <th>Quantity</th>
              <th>Budget</th>
              <th>Action</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(product, index) in productsToShow" :key="index">
              <td>{{ product.category ? product.category.name : 'N/A' }}</td>
              <td>{{ product.productId ? findProductName(product.productId) : product.productName }}</td>
              <td>{{ product.quantity || 'N/A' }}</td>
              <td>{{ product.budget || 'N/A' }}</td>
              <td>
                <v-btn v-if="!isViewMode" small color="error" @click="removeProduct(index)">
                  <v-icon>mdi-delete</v-icon>
                </v-btn>
              </td>
            </tr>
          </tbody>
        </table>
        <div class="form-group">
          <label>Priority</label>
          <select v-model="priority.value" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode">
            <option v-for="option in priority.options" :value="option.value" :key="option.value">
              {{ option.label }}
            </option>
          </select>
        </div>
        <div class="form-group">
          <label>Deal Justification</label>
          <textarea v-model="dealJustification" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode"></textarea>
        </div>
        <div class="form-group">
          <label>Deadline</label>
          <input type="datetime-local" v-model="deadline" class="border-input" :class="{'readonly-field': isViewMode}" :readonly="isViewMode">
        </div>
        <button type="submit" @click="submitQuotation" v-if="!isViewMode">Submit</button>
        <button type="submit" @click="saveDraft" v-if="!isViewMode">Save Draft</button>
        <button type="submit" @click="redirectToRequest">Return to Request</button>
      </v-card-text>
    </v-card>

  </div>
</template>

<script>


  export default {
    name: "request-quotation",
    data() {
      return {
        categories: [],
        selectedCategories: [],
        isChecked: [],
        selectedProducts: {},
        product: { category: null, productId: null, quantity: null, budget: null },
        products: [],
        productOptions: [],
        productsToShow: [],
        options: {},
        priority: {
          value: 1,
          options: [
            { value: 1, label: 'High' },
            { value: 2, label: 'Medium' },
            { value: 3, label: 'Low' }
          ]
        },
        quantity: {},
        budget: {},
        fulfilledPrice: {},
        fulfillerName: {},
        approvalStateStr: '',
        customerName: '',
        dealJustification: '',
        deadline: '',
        dialog: false,
      };
    },
    async created() {
      await this.fetchCategories();
      if (this.$route.query.view) {
        const request = JSON.parse(this.$route.query.request);
        this.populateForm(request);
      }
      else {
        await this.loadDraft();
      }
    },
    computed: {
      isViewMode() {
        return this.$route.query.view === 'true';
      },
      today() {
        const date = new Date();
        const year = date.getFullYear();
        let month = date.getMonth() + 1;
        let day = date.getDate();
        let hours = date.getHours();
        let minutes = date.getMinutes();

        month = (month < 10) ? `0${month}` : month;
        day = (day < 10) ? `0${day}` : day;
        hours = (hours < 10) ? `0${hours}` : hours;
        minutes = (minutes < 10) ? `0${minutes}` : minutes;

        return `${year}-${month}-${day}T${hours}:${minutes}`;
      }
    },
    methods: {
      addProductRow() {
        if (this.product.category && this.product.productId && this.product.quantity && this.product.budget) {
          const newProduct = { ...this.product };
          this.products.push(newProduct); 
          this.showAddedProducts(newProduct); 
          this.product.category = null;
          this.product.productId = null;
          this.product.quantity = null;
          this.product.budget = null;
          this.productOptions = [];
          this.dialog = false;
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
      async populateForm(requestData) {
        for (const productModel of requestData.requestProductsModel) {
          const categoryFound = this.categories.find((categoryFound) => categoryFound.id === productModel.productCategory.categoryId);
          if (categoryFound) {
            this.selectedCategories.push(categoryFound);
            await this.fetchProductsForCategory(categoryFound);
            const p = {
              category: categoryFound,
              productid: productModel.productId,
              quantity: productModel.quantity,
              budget: productModel.budget,
              productName: productModel.productName
            };
            this.productsToShow.push(p);
          }
        }
        this.priority.value = requestData.priority;
        this.customerName = requestData.customerName;
        this.dealJustification = requestData.dealJustification;
        this.deadline = requestData.deadline;
        this.approvalStateStr = requestData.approvalStateStr;
      },
      async fetchCategories() {
        try {
          const response = await this.$axios.get(`${this.$config.restUrl}/api/category/getcategories`);
          this.categories = response.data.data;
          for (const category of this.categories) {
            await this.fetchProductsForCategory(category);
          }
        } catch (error) {
          console.error(error);
        }
      },
      async fetchProductsForCategory(category) {
        try {
          const response = await this.$axios.get(`${this.$config.restUrl}/api/product/getproductbycategory`, { params: { categoryId: category.id } });
          this.$set(this.options, category.id, response.data.data);
        } catch (error) {
          console.error(error);
        }
      },
      async loadDraft() {
        try {
          this.selectedCategories = [];
          for (let i = 0; i < this.categories.length; i++) {

            const category = this.categories.find((c) => c.id === this.categories[i].id);
            const categoryId = category.id;
            if (localStorage.getItem("savedItem-selectedCategoryId" + categoryId) != null && localStorage.getItem("savedItem-selectedCategoryName" + categoryId)) {
              this.selectedCategories.push({ id: localStorage.getItem("savedItem-selectedCategoryId" + categoryId), name: localStorage.getItem("savedItem-selectedCategoryName" + categoryId) });
              await this.fetchProductsForCategory(category);
            }

            if (localStorage.getItem("savedItem-quantity" + categoryId) !== "null") {
              this.quantity[categoryId] = localStorage.getItem("savedItem-quantity" + categoryId);
            }

            if (localStorage.getItem("savedItem-budget" + categoryId) !== "null") {
              this.budget[categoryId] = localStorage.getItem("savedItem-budget" + categoryId);
            }

            if (localStorage.getItem("savedItem-selectedProductCategory" + categoryId) !== "null") {
              this.selectedProducts[categoryId] = localStorage.getItem("savedItem-selectedProductCategory" + categoryId);
            }
          }
          this.customerName = localStorage.getItem("savedItem-customerName", this.customerName);
          this.priority.value = localStorage.getItem("savedItem-priority", this.priority.value);
          this.dealJustification = localStorage.getItem("savedItem-dealJustification", this.dealJustification);
          this.deadline = localStorage.getItem("savedItem-deadline", this.deadline);

        } catch (error) {
          console.error(error);
        }
      },
      saveDraft() {

        localStorage.clear();

        for (const categoryIndex in this.selectedCategories) {
          localStorage.setItem("savedItem-selectedCategoryId" + this.selectedCategories[categoryIndex].id, this.selectedCategories[categoryIndex].id);
          localStorage.setItem("savedItem-selectedCategoryName" + this.selectedCategories[categoryIndex].id, this.selectedCategories[categoryIndex].name);
          for (const quantityIndex in this.quantity) {
            if (this.quantity[quantityIndex] !== null) {
              localStorage.setItem("savedItem-quantity" + this.selectedCategories[categoryIndex].id, this.quantity[quantityIndex]);
            }
          }
          for (const budgetIndex in this.quantity) {
            if (this.budget[budgetIndex] !== null) {
              localStorage.setItem("savedItem-budget" + this.selectedCategories[categoryIndex].id, this.budget[budgetIndex]);
            }
          }
          for (const productIndex in this.selectedProducts) {
            if (this.selectedProducts[productIndex] !== null) {
              localStorage.setItem("savedItem-selectedProductCategory" + this.selectedCategories[categoryIndex].id, this.selectedProducts[productIndex]);
            }
          }
        }
        localStorage.setItem("savedItem-customerName", this.customerName);
        localStorage.setItem("savedItem-priority", this.priority.value);
        localStorage.setItem("savedItem-dealJustification", this.dealJustification);
        localStorage.setItem("savedItem-deadline", this.deadline);
        this.$swal('Request draft saved');

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
            this.submitForm(selectedCategory);
          } else {
            this.$delete(this.selectedProducts, categoryId);
            this.$delete(this.quantity, categoryId);
            this.$delete(this.budget, categoryId);
            this.$delete(this.options, categoryId);
          }
        }
      },
      async submitForm(selectedCategory) {
        if (selectedCategory.id != null) {
          const categoryId = selectedCategory.id;
          const formValues = {
            categoryId,
          };
          try {
            const response = await this.$axios.get(`${this.$config.restUrl}/api/product/getproductbycategory`, { params: formValues });
            this.$set(this.options, categoryId, response.data.data);
          } catch (error) {
            console.error(error);
          }
        }

      },
      redirectToRequest() {
        this.$router.push('/request');
      },
      async submitQuotation() {
        if (this.deadline < this.today) {
          this.$swal('Error', 'Deadline should be later than today', 'error');
          return;
        }
        const quotationData = {
          ApprovalState: 20,
          Priority: this.priority,
          requestProducts: [],
        };
        console.log("productstoshow", this.productsToShow);
        for (const product in this.productsToShow) {
          const productToInsert = {
            productId: this.productsToShow[product].productId,
            quantity: this.productsToShow[product].quantity,
            budget: this.productsToShow[product].budget
          };
          quotationData.requestProducts.push(productToInsert);
        }
        try {
          const vm = this;
          await this.$axios.post(`${this.$config.restUrl}/api/request/createrequest`, {
            data: {
              segment: "string",
              approvalState: 10,
              priority: this.priority.value,
              RequestProducts: quotationData.requestProducts,
              customerName: this.customerName,
              dealJustification: this.dealJustification,
              deadline: this.deadline
            }
          }).then(response => {
            this.$swal('Request created');
            this.$router.push('/request');
            localStorage.clear();
          }).catch(err => {
            console.log(err);
            vm.$swal('Failed to submit request', err.response.data.message, 'error');
          })
        } catch (error) {
          console.log(error);
        }
      }
    }
  }
</script>

<style scoped>
  table {
    width: 100%;
    margin-top: 2rem;
    border-collapse: collapse;
  }

  th, td {
    border: 1px solid #ddd;
    padding: 8px;
    text-align: left;
  }

  tr:nth-child(even) {
    background-color: #f2f2f2;
  }
  h1 {
    margin-top: 0;
    font-size: 2rem;
    text-align: center;
  }

  .border-input {
    border: 1px solid #ccc;
    border-radius: 4px;
    padding: 0.5rem;
    width: 100%;
  }

  .form-container {
    max-width: 400px;
    padding: 2rem;
    border: 1px solid #ccc;
    border-radius: 4px;
  }

  .create-quotation-container {
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
  }

  label {
    font-weight: bold;
    margin-bottom: 0.5rem;
    color: black;
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

  .form-group {
    margin-bottom: 1rem;
    display: flex;
    justify-content: center;
    flex-direction: column;
  }

  button {
    padding: 0.5rem 1rem;
    background-color: #003399;
    color: #fff;
    border: none;
    cursor: pointer;
  }

  .readonly-field {
    background-color: #ddd;
  }

  @media (max-width: 768px) {
    form {
      max-width: 300px;
      padding: 1rem;
    }

    h1 {
      font-size: 1.5rem;
    }
  }
</style>
