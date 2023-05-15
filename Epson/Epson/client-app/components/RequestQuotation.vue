<template>
  <div class="create-quotation-container">
    <h1>Create Quotation</h1>
    <v-card class="mx-auto" width="800">
      <v-card-text>
        <div v-for="(category, index) in categories" :key="'C'+index">
          <div class="blue-checkbox">
            <input type="checkbox" v-model="selectedCategories" :value="category" @change="checkboxChanged(category)">
            <label class="category-name">{{ category.name }}</label>
          </div>
        </div>
        <div v-for="category in selectedCategories" :key="category.id">
          <div class="form-group">
            <label>Select Product</label>
            <select v-model="selectedProducts[category.id]" required class="border-input">
              <option v-for="option in options[category.id]" :value="option.id" :key="option.id">{{ option.name }}</option>
            </select>
          </div>
          <div class="form-group">
            <label>Quantity</label>
            <input v-model="quantity[category.id]" class="border-input" type="text" placeholder="Quantity">
          </div>
          <div class="form-group">
            <label>Budget</label>
            <input v-model="budget[category.id]" class="border-input" type="text" placeholder="Budget">
          </div>
        </div>
        <div class="form-group">
          <label>Priority</label>
          <input v-model="priority" class="border-input" type="text" placeholder="Priority">
        </div>
        <button type="submit" @click="submitQuotation">Submit</button>
        <button type="submit" @click="saveDraft">Save Draft</button>
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
        priority: 0,
        quantity: {},
        budget: {},
      };
    },
    created() {
      // Initialize the options object with an empty array for each category
      this.categories.forEach(category => {
        this.options[category.id] = [];
      });
      this.fetchCategories();
    },
    mounted() {
      this.loadDraft();
    },
    methods: {
      async fetchCategories() {
        try {
          await this.$axios.get(`${this.$config.restUrl}/api/category/getcategories`).then(response => {
            this.categories = response.data.data;
            this.loadDraft();

          });

        } catch (error) {
          console.error(error);
        }
      },
      loadDraft() {
        try {
          this.selectedCategories = [];

          for (const category in this.categories) {
            const categoryId = this.categories[category].id;
            if (localStorage.getItem("savedItem-selectedCategoryId" + categoryId) != null && localStorage.getItem("savedItem-selectedCategoryName" + categoryId)) {
              this.selectedCategories.push({ id: localStorage.getItem("savedItem-selectedCategoryId" + categoryId), name: localStorage.getItem("savedItem-selectedCategoryName" + categoryId) });
              this.checkboxChanged(this.categories[category]);
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
        
      },
      checkboxChanged(selectedCategory) {
        const categoryId = selectedCategory.id;

        if (this.selectedProducts[categoryId] === undefined) {
          // Initialize selectedProducts object with default values for the selected category
          this.selectedProducts[categoryId] = '';
          this.quantity[categoryId] = null;
          this.budget[categoryId] = null;
        }
        this.submitForm(selectedCategory);
      },
      async submitForm(selectedCategory) {
        if (selectedCategory.id != null) {
          const categoryId = selectedCategory.id;
          const formValues = {
            categoryId,
          };
          try {
            const response = await this.$axios.get(`${this.$config.restUrl}/api/product/getproductbycategory`, { params: formValues });
            this.$set(this.options, categoryId, response.data.data); // Use $set to update the options for the selected category
          } catch (error) {
            console.error(error);
          }
        }
        
      },
      async submitQuotation() {
        const quotationData = {
          ApprovalState: 20,
          Priority: this.priority,
          requestProducts: [],
        };
        for (const categoryId in this.selectedProducts) {
          const productId = this.selectedProducts[categoryId];
          if (productId !== '' && productId !== null ) {
            const product = {
              id: productId,
              fulfillerId: "string",
              quantity: this.quantity[categoryId],
              budget: this.budget[categoryId],
            };
            quotationData.requestProducts.push(product);
          }
        }
        try {
          const vm = this;
          await this.$axios.post(`${this.$config.restUrl}/api/request/createrequest`, {
            data: {
              segment: "string",
              approvalState: 20,
              priority: this.priority,
              RequestProducts: quotationData.requestProducts

            }
          }).then(response => {
            console.log('res', response);
            this.getDataFromApi();
          }).catch(err => {
            console.log(err);
            console.log(err.response);
            vm.$swal('Failed to edit', err.response.data.message, 'error');
          })
        } catch (error) {
          console.log(error);
        }
      }
    }
  };
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
