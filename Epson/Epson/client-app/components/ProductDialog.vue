<template>
  <v-dialog v-model="localDialogProduct" max-width="500px">
    <v-card>
      <v-card-title>
        <span class="headline">{{ isEditMode ? 'Edit Product' : 'Add Product' }}</span>
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
        <v-btn color="primary" @click="isEditMode ? onEditProduct() : onAddProduct()">
          {{ isEditMode ? 'Save Changes' : 'Add' }}
        </v-btn>
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
        categories: [],
        isEditMode: false
      };
    },
    watch: {
      dialogProduct(newVal) {
        this.localDialogProduct = newVal;
      },
      localDialogProduct(newVal) {
        this.$emit('update:dialogProduct', newVal);
        if (!newVal) { 
          this.isEditMode = false;
        }
      },
      product: {
        immediate: true,
        handler(newVal) {
          console.log("Product prop updated in ProductDialog:", newVal);
          this.localProduct = { ...newVal };
          this.isEditMode = newVal && newVal.id != null;
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
        } else {
          this.localProductOptions = [];
        }
      },
      setEditMode(isEdit, product) {
        this.isEditMode = isEdit;
        this.localProduct = { ...product };
        console.log("edited product", product);
        if (product.productId != null) {
          this.localProduct.category = product.category;
          this.updateProductOptions(); 
        }
      },
      onEditProduct() {
        console.log("edit");
        if (this.validateProduct()) {
          this.$emit('edit-product', this.localProduct);
          this.resetLocalProduct();
          this.localDialogProduct = false;
        } else {
          this.$swal('Error', 'Please fill out all product fields', 'error');
        }
      },
      onAddProduct() {
        if (this.validateProduct()) {
          this.$emit('add-product', this.localProduct);
          this.resetLocalProduct();
          this.localDialogProduct = false;
        } else {
          this.$swal('Error', 'Please fill out all product fields', 'error');
        }
      },
      validateProduct() {
        return this.localProduct.category && this.localProduct.productId &&
          this.localProduct.quantity && this.localProduct.dealerPrice &&
          this.localProduct.endUserPrice;
      },
      resetLocalProduct() {
        this.localProduct = {
          category: null,
          productId: null,
          quantity: null,
          distyPrice: null,
          dealerPrice: null,
          endUserPrice: null,
        };
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
