<template>
  <div class="container">
    <v-card class="mx-auto mt-10" width="800">
      <v-card-title>Create Quotation</v-card-title>
      <v-card-text>
        <div v-for="category in categories" :key="category.id">
          <v-checkbox v-model="selectedCategories" :label="category.name" :value="category" @change="checkboxChanged(category)"></v-checkbox>
        </div>
        <div v-for="category in selectedCategories" :key="category.id">
          <select v-model="selectedProducts[category.id]" required class="border-input">
            <option value="">Select a product</option>
            <option v-for="option in options[category.id]" :value="option.id" :key="option.id">{{ option.name }}</option>
          </select>
          <v-text-field v-model="quantity[category.id]" label="Quantity"></v-text-field>
          <v-text-field v-model="budget[category.id]" label="Budget"></v-text-field>
        </div>
        <v-btn color="primary" @click="submitQuotation" class="border-input">Submit</v-btn>
      </v-card-text>
    </v-card>
  </div>
</template>

<script>
  export default {
    name: "product-list",
    data() {
      return {
        categories: [],
        selectedCategories: [],
        selectedProducts: {},
        options: {},
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
          Priority: 1,
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
