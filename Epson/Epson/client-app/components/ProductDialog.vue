<template>
  <v-dialog v-model="localDialogProduct" max-width="500px">
    <v-card>
      <v-card-title>
        <span class="headline">{{ isEditMode ? 'Main Unit' : 'Main Unit' }}</span>
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
        this.localProduct.status = 0;
        if (product.productId != null) {
          this.localProduct.category = product.category;
          this.updateProductOptions();
        }
      },
      onEditProduct() {
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
      await this.updateCategories();
      if (this.localProduct && this.localProduct.category) {
        await this.updateProductOptions();
      }
    }
  };
</script>

<style scoped>
  .products-title {
    font-size: 2em;
    text-align: center;
  }

  .custom-radio {
    border: 1px solid #000;
    margin: 2px;
    width: 1.2em;
    height: 1.2em;
    border-radius: 50%;
  }

  .header-row {
    background-color: #C0C0C0;
  }

    .header-row th {
      text-align: center;
      vertical-align: middle;
    }

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

