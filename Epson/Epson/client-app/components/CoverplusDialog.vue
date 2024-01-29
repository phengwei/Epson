<template>
  <v-dialog v-model="localDialogCoverplus" max-width="500px">
    <v-card>
      <v-card-title>
        <span class="headline">{{ isEditMode ? 'Coverplus' : 'Coverplus' }}</span>
      </v-card-title>
      <v-card-text>
        <div class="form-group">
          <label>Product Categories</label>
          <select v-model="localCoverplus.category" @change="updateCoverplusOptions" :class="{'readonly-field': isViewMode}" :readonly="isViewMode">
            <option v-for="category in categories" :value="category" :key="category.id">{{ category.name }}</option>
          </select>
          <label>Product</label>
          <select v-model="localCoverplus.productId" :class="{'readonly-field': isViewMode}" :readonly="isViewMode">
            <option v-for="option in localCoverplusOptions" :value="option.id" :key="option.id">{{ option.name }}</option>
          </select>
          <label>Quantity</label>
          <input v-model="localCoverplus.quantity" class="border-input" type="number" min="1" :class="{'readonly-field': isViewMode}" :readonly="isViewMode">
          <label>Disty Price</label>
          <input v-model="localCoverplus.distyPrice" class="border-input" type="number" min="1" :class="{'readonly-field': isViewMode}" :readonly="isViewMode">
          <label>Dealer Price</label>
          <input v-model="localCoverplus.dealerPrice" class="border-input" type="number" min="1" :class="{'readonly-field': isViewMode}" :readonly="isViewMode">
          <label>End User Price</label>
          <input v-model="localCoverplus.endUserPrice" class="border-input" type="number" min="1" :class="{'readonly-field': isViewMode}" :readonly="isViewMode">
        </div>
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="primary" @click="isEditMode ? onEditCoverplus() : onAddCoverplus()">
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
      dialogCoverplus: Boolean,
      coverplus: Object,
      isViewMode: Boolean
    },
    data() {
      return {
        localDialogCoverplus: this.dialogCoverplus,
        localCoverplus: { ...this.coverplus },
        localCoverplusOptions: [],
        categories: [],
        isEditMode: false
      };
    },
    watch: {
      dialogCoverplus(newVal) {
        this.localDialogCoverplus = newVal;
      },
      localDialogCoverplus(newVal) {
        this.$emit('update:dialogCoverplus', newVal);
        if (!newVal) {
          this.isEditMode = false;
        }
      },
      coverplus: {
        immediate: true,
        handler(newVal) {
          this.localCoverplus = { ...newVal };
          this.isEditMode = newVal && newVal.id != null;
        },
        deep: true,
      },
      coverplusOptions: {
        handler(newVal) {
          this.localCoverplusOptions = [...newVal];
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
      async updateCoverplusOptions() {
        const category = this.localCoverplus.category;

        if (category) {
          try {
            const response = await this.$axios.get(`${this.$config.restUrl}/api/product/getproductbycategory`, { params: { categoryId: category.id } });
            this.localCoverplusOptions = response.data.data;
          } catch (error) {
            console.error(error);
          }
        } else {
          this.localCoverplsOptions = [];
        }
      },
      setEditMode(isEdit, coverplus) {
        this.isEditMode = isEdit;
        this.localCoverplus = { ...coverplus };
        this.localCoverplus.status = 0;
        if (coverplus.productId != null) {
          this.localCoverplus.category = coverplus.category;
          this.updateCoverplusOptions();
        }
      },
      onEditCoverplus() {
        if (this.validateCoverplus()) {
          this.$emit('edit-coverplus', this.localCoverplus);
          this.resetLocalCoverplus();
          this.localDialogCoverplus = false;
        } else {
          this.$swal('Error', 'Please fill out all coverplus fields', 'error');
        }
      },
      onAddCoverplus() {
        if (this.validateCoverplus()) {
          this.$emit('add-coverplus', this.localCoverplus);
          this.resetLocalCoverplus();
          this.localDialogCoverplus = false;
        } else {
          this.$swal('Error', 'Please fill out all coverplus fields', 'error');
        }
      },
      validateCoverplus() {
        return this.localCoverplus.category && this.localCoverplus.productId
          && this.localCoverplus.quantity && this.localCoverplus.dealerPrice
          && this.localCoverplus.endUserPrice;
      },
      resetLocalCoverplus() {
        this.localCoverplus = {
          category: null,
          productId: null,
          quantity: null,
          distyPrice: null,
          dealerPrice: null,
          endUserPrice: null,
        };
      },
      onCancel() {
        this.localDialogCoverplus = false;
      }
    },
    async created() {
      if (this.localCoverplus && this.localCoverplus.category) {
        this.updateCoverplusOptions();
      }
      await this.updateCategories();
      if (this.localCoverplus && this.localCoverplus.category) {
        await this.updateCoverplusOptions();
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


