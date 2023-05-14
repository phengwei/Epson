<template>
  <div class="create-quotation-container">
    <h1>Create Quotation</h1>
    <v-card class="mx-auto" width="800">
      <v-card-text>
        <div v-for="category in categories" :key="category.id">
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
    methods: {
      async fetchCategories() {
        try {
          const response = await this.$axios.get(`${this.$config.restUrl}/api/category/getcategories`);
          this.categories = response.data.data;
        } catch (error) {
          console.error(error);
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
      },
      async submitQuotation() {
        const quotationData = {
          ApprovalState: 10,
          Priority: this.priority,
          requestProducts: [],
        };
        for (const categoryId in this.selectedProducts) {
          const productId = this.selectedProducts[categoryId];
          if (productId !== '') {
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
          console.log(quotationData);
          await this.$axios.post(`${this.$config.restUrl}/api/request/createrequest`, {
            data: {
              segment: "string",
              approvalState: 10,
              priority: 1,
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
