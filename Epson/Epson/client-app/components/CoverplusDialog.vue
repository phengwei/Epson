<template>
  <v-dialog v-model="localDialogCoverplus" max-width="500px">
    <v-card>
      <v-card-title>
        <span class="headline">Add Coverplus</span>
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
        <v-btn color="primary" @click="onAddCoverplus">Add</v-btn>
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
        categories: []
      };
    },
    watch: {
      dialogCoverplus(newVal) {
        this.localDialogCoverplus = newVal;
      },
      localDialogCoverplus(newVal) {
        this.$emit('update:dialogCoverplus', newVal);
      },
      coverplus: {
        handler(newVal) {
          this.localCoverplus = { ...newVal };
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
        }
      },
      onAddCoverplus() {
        if (this.localCoverplus.category && this.localCoverplus.productId && this.localCoverplus.quantity && this.localCoverplus.dealerPrice && this.localCoverplus.endUserPrice) {
          this.$emit('add-coverplus', this.localCoverplus);
          this.localCoverplus = {
            category: null,
            productId: null,
            quantity: null,
            distyPrice: null,
            dealerPrice: null,
            endUserPrice: null,
          };
          this.localDialogCoverplus = false;
        } else {
          this.$swal('Error', 'Please fill out all coverplus fields', 'error');
        }
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
