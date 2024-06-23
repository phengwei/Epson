<template>
  <div class="d-flex justify-content-center align-items-center vh-100" data-app="true" v-if="loggedInUser.roles.includes('Product') || loggedInUser.roles.includes('Admin') || loggedInUser.roles.includes('Director')">
    <v-card class="mx-auto card-round" style="width: 90%; padding: 20px;">
      <v-card-title class="d-flex justify-content-between align-items-center">
        <v-toolbar flat>
          <v-toolbar-title><h2 class="blue-text big-bold">PRODUCTS</h2></v-toolbar-title>
          <v-spacer></v-spacer>
          <v-text-field v-model="search"
                        prepend-inner-icon="mdi-magnify"
                        placeholder="Search by product name"
                        solo
                        hide-details
                        flat
                        dense
                        class="search-bar"></v-text-field>
        </v-toolbar>

        <v-tabs class="mt-4" v-model="tab" background-color="white">
          <v-tab v-for="(item, index) in tabItems" :key="index" :class="{'blue-text--active': tab === index}">
            {{ item }}
          </v-tab>
          <v-spacer></v-spacer>
          <v-btn class="mr-5 blue-button" color="primary" dark @click="dialog = true" v-if="loggedInUser.roles.includes('Product') || loggedInUser.roles.includes('Admin')">
            <v-icon left>mdi-plus</v-icon>
            ADD PRODUCT
          </v-btn>
        </v-tabs>

        <v-dialog v-model="dialog" max-width="500px">
          <v-card>
            <v-card-title v-if="loggedInUser.roles.includes('Product') || loggedInUser.roles.includes('Coverplus') || loggedInUser.roles.includes('Admin')">
              <span class="text-h5 blue-text big-bold">{{ formTitle }}</span>
            </v-card-title>
            <v-card-text>
              <label>Product Category</label>
              <div v-for="category in categories" :key="category.id" class="role-checkbox">
                <div class="blue-checkbox">
                  <input type="checkbox" v-model="selectedCategories" :value="category" class="styled-checkbox">
                  <label class="category-name">{{ category.name }}</label>
                </div>
              </div>
              <div class="form-group">
                <label>Product Name</label>
                <input v-model="editedItem.name" class="border-input" label="Product name" required></input>
              </div>
              <div class="form-group">
                <label>Bottom Price (RM)</label>
                <input type="number" v-model="editedItem.price" class="border-input" label="Bottom Price" required></input>
              </div>
              <div class="form-group">
                <label>Dealer Price (RM)</label>
                <input type="number" v-model="editedItem.dealerPrice" class="border-input" label="Dealer Price" required></input>
              </div>
            </v-card-text>
            <v-card-actions>
              <v-spacer></v-spacer>
              <v-btn color="blue darken-1" text @click="close">Cancel</v-btn>
              <v-btn color="blue darken-1" text @click="save">Save</v-btn>
            </v-card-actions>
          </v-card>
        </v-dialog>
      </v-card-title>
      <v-card-text>
        <v-data-table :headers="headers"
                      :items="filteredProducts"
                      :options.sync="options"
                      :items-per-page="5"
                      :loading="loading"
                      class="elevation-1">
          <template v-slot:item.actions="{ item }">
            <v-icon small class="mr-2" @click="editItem(item)">mdi-pencil</v-icon>
            <v-icon small v-if="item.isActive" @click="deactivateItemConfirm(item)">mdi-eye-off</v-icon>
            <v-icon small v-if="!item.isActive" @click="reactivateItemConfirm(item)">mdi-eye</v-icon>
          </template>
        </v-data-table>
      </v-card-text>
    </v-card>
  </div>
</template>

<script>
  import { mapGetters } from 'vuex';
  import Swal from 'sweetalert2';
  import moment from 'moment';

  export default {
    name: 'ProductTable',
    middleware: "auth",
    computed: {
      ...mapGetters(['isAuthenticated', 'loggedInUser']),
      formTitle() {
        return this.editedIndex === -1 ? 'ADD PRODUCT' : 'EDIT PRODUCT'
      },
      selectedCategoryList() {
        return this.categories.filter((category) => this.selectedCategories.includes(category.id));
      },
      filteredProducts() {
        let filtered = this.products;
        if (this.search.trim() !== '') {
          filtered = filtered.filter(product => {
            return product.name.toLowerCase().includes(this.search.toLowerCase());
          });
        }
        return filtered;
      }
    },
    data() {
      return {
        error: null,
        dialog: false,
        dialogDelete: false,
        headers: [
          {
            text: 'ID',
            align: ' d-none',
            value: 'id',
          },
          {
            text: 'Product Name',
            align: 'start',
            value: 'name',
          },
          { text: 'Bottom Price', value: 'price' },
          { text: 'Dealer Price', value: 'dealerPrice' },
          { text: 'Created On', value: 'createdOnUTC' },
          { text: 'Status', value: 'status' },
          { text: 'Actions', value: 'actions', sortable: false },
        ],
        options: {},
        products: [],
        categories: [],
        loading: true,
        search: '',
        selectedCategories: [],
        totalProducts: 0,
        editedIndex: -1,
        editedItem: {
          id: 0,
          name: '',
          price: null,
          dealerPrice: null
        },
        defaultItem: {
          name: '',
          price: null,
          dealerPrice: null
        },
      }
    },
    watch: {
      options: {
        handler() {
          this.getProducts()
        },
        deep: true,
      },
      dialog(val) {
        val || this.close()
      },
      dialogDelete(val) {
        val || this.closeDelete()
      },
    },
    created() {
      this.getCategoryFromApi();
      this.getProducts();
    },
    methods: {
      getProducts() {
        this.loading = true;
        this.$axios.get(`${this.$config.restUrl}/api/product/getproducts`).then(result => {
          this.products = result.data.data.map(product => {
            return {
              ...product,
              createdOnUTC: moment(product.createdOnUTC).format('DD MMM YY HH:mm')

            };
          }).sort((a, b) => moment(b.createdOnUTC, 'DD MMM YY HH:mm').valueOf() - moment(a.createdOnUTC, 'DD MMM YY HH:mm').valueOf());

        });
      },
      async getCategoryFromApi() {
        this.loading = true
        await this.$axios.get(`${this.$config.restUrl}/api/category/getcategories`).then(result => {
          this.categories = result.data.data
          this.categoriesLength = result.data.data.length
          this.loading = false
        })
      },
      editItem(item) {
        this.editedIndex = this.products.indexOf(item)
        this.editedItem = Object.assign({}, item)
        this.selectedCategories = this.categories.filter(category =>
          item.productCategoriess.find(pc => pc.categoryId === category.id)
        )
        this.dialog = true
      },
      async deactivateItem(item) {
        const vm = this;
        try {
          await this.$axios.post(`${this.$config.restUrl}/api/product/deactivateproduct?id=${item.id}`).then(response => {
            this.getProducts();
          }).catch(function (error) {
            console.log('vm error', error.response);
            vm.$swal('Failed to deactivate', error.response.data.errorList[0], 'error');
          })
        } catch (err) {
          console.log('try', err);
        }
      },
      deactivateItemConfirm(item) {
        this.$swal({
          title: 'Are you sure to deactivate the product?',
          icon: 'warning',
          showCancelButton: true,
          confirmButtonColor: '#3085d6',
          cancelButtonColor: '#d33',
          confirmButtonText: 'Yes, deactivate it!'
        }).then((result) => {
          if (result.isConfirmed) {
            this.deactivateItem(item);
          }
        })
      },
      async reactivateItem(item) {
        const vm = this;
        try {
          await this.$axios.post(`${this.$config.restUrl}/api/product/reactivateProduct?id=${item.id}`).then(response => {
            this.getProducts();
          }).catch(function (error) {
            console.log('vm error', error.response);
            vm.$swal('Failed to reactivate', error.response.data.errorList[0], 'error');
          })
        } catch (err) {
          console.log('try', err);
        }
      },
      reactivateItemConfirm(item) {
        this.$swal({
          title: 'Are you sure to reactivate the product?',
          icon: 'primary',
          showCancelButton: true,
          confirmButtonColor: '#3085d6',
          cancelButtonColor: '#d33',
          confirmButtonText: 'Yes, reactivate it!'
        }).then((result) => {
          if (result.isConfirmed) {
            this.reactivateItem(item);
          }
        })
      },
      close() {
        this.dialog = false
        this.$nextTick(() => {
          this.editedItem = Object.assign({}, this.defaultItem);
          this.selectedCategories = [];
          this.editedIndex = -1;
        })
      },

      closeDelete() {
        this.dialogDelete = false
        this.$nextTick(() => {
          this.editedItem = Object.assign({}, this.defaultItem);
          this.selectedCategories = [];
          this.editedIndex = -1;
        })
      },
      async save() {
        if (!this.editedItem.name) {
          Swal.fire(
            'Error!',
            'Product Name is required.',
            'error'
          );
          return;
        }
        if (this.selectedCategories.length === 0) {
          Swal.fire(
            'Error!',
            'You must select at least one category.',
            'error'
          );
          return;
        }
        if (!this.editedItem.price || isNaN(this.editedItem.price)) {
          Swal.fire(
            'Error!',
            'Bottom Price is required.',
            'error'
          );
          return;
        }
        const vm = this;
        const result = await Swal.fire({
          title: 'Are you sure?',
          text: "You won't be able to revert this!",
          icon: 'warning',
          showCancelButton: true,
          confirmButtonColor: '#3085d6',
          cancelButtonColor: '#d33',
          confirmButtonText: 'Yes, save it!'
        });

        if (result.isConfirmed) {
          try {
            if (this.editedIndex > -1) {
              await this.$axios.post(`${this.$config.restUrl}/api/product/editproduct`, {
                data: {
                  id: this.editedItem.id,
                  name: this.editedItem.name,
                  price: this.editedItem.price,
                  dealerPrice: this.editedItem.dealerPrice,
                  productcategories: this.selectedCategories.map(category => ({
                    categoryid: category.id,
                    productId: this.editedItem.id
                  }))
                }
              });
              this.getProducts();
              Swal.fire(
                'Saved!',
                'Your product has been updated.',
                'success'
              );
            } else {
              await this.$axios.post(`${this.$config.restUrl}/api/product/addproduct`, {
                data: {
                  name: this.editedItem.name,
                  price: this.editedItem.price,
                  dealerPrice: this.editedItem.dealerPrice,
                  productcategories: this.selectedCategories.map(category => ({
                    categoryid: category.id,
                    productId: this.editedItem.id
                  }))
                }
              });

              this.getProducts();
              Swal.fire(
                'Saved!',
                'Your product has been added.',
                'success'
              );
            }
          } catch (err) {
            console.log(err);
            console.log(err.response);
            vm.$swal('Failed to save', err.response.data.message, 'error');
          }
          this.loading = false;
          this.close();
        }
      },
    },
  }
</script>

<style scoped>
  @import '~@/../wwwroot/css/general-table.css';
</style>
