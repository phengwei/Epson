<template>
  <div class="create-quotation-container">
    <h1>Pricing Request</h1>
    <v-card class="mx-auto" width="800">
      <v-card-text>
        <div class="form-group">
          <label>Customer Name</label>
          <input type="text" v-model="customerName" class="border-input" :readonly="isViewMode">
        </div>
        <label>Product Categories</label>
        <div v-for="(category, index) in categories" :key="'C'+index">
          <div class="blue-checkbox">
            <input type="checkbox" v-model="selectedCategories" :value="category" @change="checkboxChanged(category)" :disabled="isViewMode">
            <label class="category-name">{{ category.name }}</label>
          </div>
        </div>
        <div v-for="category in selectedCategories" :key="category.id">
          <div class="form-group">
            <label>{{ category.name }}</label>
            <select v-if="!isViewMode" v-model="selectedProducts[category.id]" required class="border-input">
              <option v-for="option in options[category.id]" :value="option.id" :key="option.id">{{ option.name }}</option>
            </select>
            <span v-else>{{ selectedProducts[category.id] }}</span>
          </div>
          <div class="form-group">
            <label>Quantity</label>
            <input v-model="quantity[category.id]" class="border-input" type="number" min="1" :readonly="isViewMode">
          </div>
          <div class="form-group">
            <label>Budget</label>
            <input v-model="budget[category.id]" class="border-input" type="number" min="1" :readonly="isViewMode">
          </div>
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
          <textarea v-model="dealJustification" class="border-input" :readonly="isViewMode"></textarea>
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
        customerName: '',
        dealJustification: '',
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
    },
    methods: {
      async populateForm(requestData) {
        for (const productModel of requestData.requestProductsModel) {
          const category = this.categories.find((category) => category.id === productModel.productCategory.categoryId);
          if (category) {
            this.selectedCategories.push(category);
            await this.fetchProductsForCategory(category);
            this.selectedProducts[category.id] = productModel.productName;
            this.quantity[category.id] = productModel.quantity;
            this.budget[category.id] = productModel.budget;
          }
        }
        console.log("requestdata", requestData);
        this.priority.value = requestData.priority;
        this.customerName = requestData.customerName;
        this.dealJustification = requestData.dealJustification;
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
              customerName: this.customerName[categoryId],
              dealJustification: this.dealJustification[categoryId]
            };
            quotationData.requestProducts.push(product);
          }
        }
        try {
          const vm = this;
          await this.$axios.post(`${this.$config.restUrl}/api/request/createrequest`, {
            data: {
              segment: "string",
              approvalState: 10,
              priority: this.priority.value,
              RequestProducts: quotationData.requestProducts

            }
          }).then(response => {
            this.$swal('Request created');
            this.$router.push('/request');
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
