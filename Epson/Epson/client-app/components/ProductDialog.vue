<template>
  <v-dialog v-model="localDialogProduct" max-width="500px">
    <v-card>
      <v-card-title>
        <span class="headline">Add Product</span>
      </v-card-title>
      <v-card-text>
        <div class="form-group">
          <label>Product Categories</label>
          <select v-model="localProduct.category" @change="updateProductOptions" :class="{'readonly-field': isViewMode}" :readonly="isViewMode">
            <option v-for="category in categories" :value="category" :key="category.id">{{ category.name }}</option>
          </select>
          <label>Product</label>
          <select v-model="localProduct.productId" :class="{'readonly-field': isViewMode}" :readonly="isViewMode">
            <option v-for="option in localProductOptions" :value="option.id" :key="option.id">{{ option.name }}</option>
          </select>
          <label>Quantity</label>
          <input v-model="localProduct.quantity" class="border-input" type="number" min="1" :class="{'readonly-field': isViewMode}" :readonly="isViewMode">
          <label>Disty Price</label>
          <input v-model="localProduct.distyPrice" class="border-input" type="number" min="1" :class="{'readonly-field': isViewMode}" :readonly="isViewMode">
          <label>Dealer Price</label>
          <input v-model="localProduct.dealerPrice" class="border-input" type="number" min="1" :class="{'readonly-field': isViewMode}" :readonly="isViewMode">
          <label>End User Price</label>
          <input v-model="localProduct.endUserPrice" class="border-input" type="number" min="1" :class="{'readonly-field': isViewMode}" :readonly="isViewMode">
        </div>
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="primary" @click="onAddProduct">Add</v-btn>
        <v-btn color="secondary" @click="onCancel">Cancel</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script>
  export default {
    props: {
      dialogProduct: Boolean,
      product: Object,
      isViewMode: Boolean
    },
    data() {
      return {
        localDialogProduct: this.dialogProduct,
        localProduct: { ...this.product },
        localProductOptions: [],
        categories: []
      };
    },
    watch: {
      dialogProduct(newVal) {
        this.localDialogProduct = newVal;
      },
      localDialogProduct(newVal) {
        this.$emit('update:dialogProduct', newVal);
      },
      product: {
        handler(newVal) {
          this.localProduct = { ...newVal };
        },
        deep: true,
      },
      productOptions: {
        handler(newVal) {
          this.localProductOptions = [...newVal];
        },
        deep: true,
      }
    },
    methods: {
      async updateCategories() {
        try {
          const response = await this.$axios.get(`${this.$config.restUrl}/api/category/getvalidcategories`);
          this.categories = response.data.data;
        } catch (error) {
          console.error(error);
        }
      },
      async updateProductOptions() {
        const category = this.localProduct.category;

        if (category) {
          try {
            const response = await this.$axios.get(`${this.$config.restUrl}/api/product/getproductbycategory`, { params: { categoryId: category.id } });
            this.localProductOptions = response.data.data;
          } catch (error) {
            console.error(error);
          }
        }
      },
      onAddProduct() {
        if (this.localProduct.category && this.localProduct.productId && this.localProduct.quantity && this.localProduct.dealerPrice && this.localProduct.endUserPrice) {
          this.$emit('add-product', this.localProduct);
          this.localProduct = {
            category: null,
            productId: null,
            quantity: null,
            distyPrice: null,
            dealerPrice: null,
            endUserPrice: null,
          };
          this.localDialogProduct = false;
        } else {
          this.$swal('Error', 'Please fill out all product fields', 'error');
        }
      },
      onCancel() {
        this.localDialogProduct = false;
      }
    },
    async created() {
      if (this.localProduct && this.localProduct.category) {
        this.updateProductOptions();
      }
      await this.updateCategories();
      if (this.localProduct && this.localProduct.category) {
        await this.updateProductOptions();
      }
    }
  };
</script>
