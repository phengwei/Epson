<template>
    <div class="d-flex justify-content-center align-items-center vh-100" data-app="true" v-if="loggedInUser.roles.includes('Product') || loggedInUser.roles.includes('Admin') || loggedInUser.roles.includes('Director')">
        <v-card class="mx-auto card-round" style="width: 90%; padding: 20px;">
            <v-card-title class="d-flex justify-content-between align-items-center">
                <v-toolbar flat>
                    <v-toolbar-title><h2 class="blue-text big-bold">PRODUCTS</h2></v-toolbar-title>
                    <v-spacer></v-spacer>
                    <v-text-field v-model="search"
                                  append-icon="mdi-magnify"
                                  placeholder="Search by product name"
                                  solo
                                  hide-details
                                  flat
                                  dense
                                  class="search-bar"></v-text-field>
                </v-toolbar>

                <v-tabs class="mt-4" background-color="white">
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
                          <div class="form-group">
                            <label>Product Category</label>
                            <select v-model="selectedCategory" class="border-input">
                              <option v-for="category in categories" :value="category.id" :key="category.id">{{ category.name }}</option>
                            </select>
                          </div>
                          <div class="form-group">
                            <label>Product SKU</label>
                            <input v-model="editedItem.sku" class="border-input" required></input>
                          </div>
                          <div class="form-group">
                            <label>Product Name</label>
                            <input v-model="editedItem.name" class="border-input" required></input>
                          </div>
                          <div class="form-group">
                            <label>Bottom Price (RM)</label>
                            <input type="number" v-model="editedItem.price" class="border-input" required></input>
                          </div>
                          <div class="form-group">
                            <label>Dealer Price (RM)</label>
                            <input type="number" v-model="editedItem.dealerPrice" class="border-input" required></input>
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
                  <v-tooltip bottom>
                    <template v-slot:activator="{ on, attrs }">
                      <v-icon small class="mr-2" v-bind="attrs" v-on="on" @click="editItem(item)">mdi-pencil</v-icon>
                    </template>
                    <span>Edit Item</span>
                  </v-tooltip>

                  <v-tooltip bottom>
                    <template v-slot:activator="{ on, attrs }">
                      <v-icon small v-if="item.isActive" class="mr-2" v-bind="attrs" v-on="on" @click="deactivateItemConfirm(item)">mdi-eye-off</v-icon>
                    </template>
                    <span>Deactivate Item</span>
                  </v-tooltip>

                  <v-tooltip bottom>
                    <template v-slot:activator="{ on, attrs }">
                      <v-icon small v-if="!item.isActive" class="mr-2" v-bind="attrs" v-on="on" @click="reactivateItemConfirm(item)">mdi-eye</v-icon>
                    </template>
                    <span>Reactivate Item</span>
                  </v-tooltip>
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
            return this.editedIndex === -1 ? 'ADD PRODUCT' : 'EDIT PRODUCT';
        },
        filteredProducts() {
          let filtered = this.products;
          if (this.search.trim() !== '') {
            const searchTerm = this.search.toLowerCase();
            filtered = filtered.filter(product => {
              return product.name.toLowerCase().includes(searchTerm) || product.sku.toLowerCase().includes(searchTerm);
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
                  { text: 'ID', align: ' d-none', value: 'id' },
                  { text: 'SKU', align: 'start', value: 'sku' },
                  { text: 'Product Name', align: 'start', value: 'name' },
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
              selectedCategory: null,
              editedIndex: -1,
              editedItem: {
                id: 0,
                sku: '',
                  name: '',
                  price: null,
                  dealerPrice: null
              },
              defaultItem: {
                  name: '',
                  price: null,
                  dealerPrice: null
              },
          };
      },
      watch: {
          options: {
              handler() {
                  this.getProducts();
              },
              deep: true,
          },
          dialog(val) {
              val || this.close();
          },
          dialogDelete(val) {
              val || this.closeDelete();
          },
      },
      created() {
          this.getCategoryFromApi();
          this.getProducts();
      },
      mounted() {
        this.modifySelectInputs();
      },
      methods: {
        modifySelectInputs() {
          const vSelects = this.$el.querySelectorAll('.v-text-field__slot');
          vSelects.forEach(vSelect => {
            const inputElement = vSelect.querySelector('input[type="text"]');
            if (inputElement) {
              inputElement.removeAttribute('type');
            }
          });
        },
          getProducts() {
              this.loading = true;
              this.$axios.get(`${this.$config.restUrl}/api/product/getproducts`).then(result => {
                  this.products = result.data.data.map(product => {
                      return {
                          ...product,
                          createdOnUTC: moment(product.createdOnUTC).format('DD MMM YY HH:mm')
                      };
                  }).sort((a, b) => moment(b.createdOnUTC, 'DD MMM YY HH:mm').valueOf() - moment(a.createdOnUTC, 'DD MMM YY HH:mm').valueOf());
                  this.loading = false;
              });
          },
          async getCategoryFromApi() {
              this.loading = true;
              try {
                  const result = await this.$axios.get(`${this.$config.restUrl}/api/category/getcategories`);
                  this.categories = result.data.data;
              } catch (error) {
                  console.error(error);
              } finally {
                  this.loading = false;
              }
          },
          async fetchProductsByCategory(categoryId) {
              try {
                  const result = await this.$axios.get(`${this.$config.restUrl}/api/product/getproductsbycategory`, {
                      params: { categoryId }
                  });
                  this.products = result.data.data;
              } catch (error) {
                  console.error(error);
              }
          },
          editItem(item) {
              this.editedIndex = this.products.indexOf(item);
              this.editedItem = Object.assign({}, item);
              this.selectedCategory = item.productCategoriess[0].categoryId;
              this.dialog = true;
          },
          async deactivateItem(item) {
              try {
                  await this.$axios.post(`${this.$config.restUrl}/api/product/deactivateproduct?id=${item.id}`);
                  this.getProducts();
              } catch (error) {
                  Swal.fire('Failed to deactivate', error.response.data.errorList[0], 'error');
              }
          },
          deactivateItemConfirm(item) {
              Swal.fire({
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
              });
          },
          async reactivateItem(item) {
              try {
                  await this.$axios.post(`${this.$config.restUrl}/api/product/reactivateProduct?id=${item.id}`);
                  this.getProducts();
              } catch (error) {
                  Swal.fire('Failed to reactivate', error.response.data.errorList[0], 'error');
              }
          },
          reactivateItemConfirm(item) {
              Swal.fire({
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
              });
          },
          close() {
              this.dialog = false;
              this.$nextTick(() => {
                  this.editedItem = Object.assign({}, this.defaultItem);
                  this.selectedCategory = null;
                  this.editedIndex = -1;
              });
          },
          closeDelete() {
              this.dialogDelete = false;
              this.$nextTick(() => {
                  this.editedItem = Object.assign({}, this.defaultItem);
                  this.selectedCategory = null;
                  this.editedIndex = -1;
              });
          },
          async save() {
              if (!this.editedItem.name) {
                  Swal.fire('Error!', 'Product Name is required.', 'error');
                  return;
              }
              if (!this.selectedCategory) {
                  Swal.fire('Error!', 'You must select a category.', 'error');
                  return;
              }
              if (!this.editedItem.price || isNaN(this.editedItem.price)) {
                  Swal.fire('Error!', 'Bottom Price is required.', 'error');
                  return;
              }
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
                                sku: this.editedItem.sku,
                                  id: this.editedItem.id,
                                  name: this.editedItem.name,
                                  price: this.editedItem.price,
                                  dealerPrice: this.editedItem.dealerPrice,
                                  productcategories: [{
                                      categoryid: this.selectedCategory,
                                      productId: this.editedItem.id
                                  }]
                              }
                          });
                          Swal.fire('Saved!', 'Your product has been updated.', 'success');
                      } else {
                          await this.$axios.post(`${this.$config.restUrl}/api/product/addproduct`, {
                            data: {
                                sku: this.editedItem.sku,
                                  name: this.editedItem.name,
                                  price: this.editedItem.price,
                                  dealerPrice: this.editedItem.dealerPrice,
                                  productcategories: [{
                                      categoryid: this.selectedCategory,
                                      productId: this.editedItem.id
                                  }]
                              }
                          });
                          Swal.fire('Saved!', 'Your product has been added.', 'success');
                      }
                      this.getProducts();
                  } catch (error) {
                      Swal.fire('Failed to save', error.response.data.message, 'error');
                  } finally {
                      this.loading = false;
                      this.close();
                  }
              }
          },
      },
  };
</script>

<style scoped>
    @import '~@/../wwwroot/css/general-table.css';

    input[type="text"], select {
        border: 1px solid #ced4da;
        border-radius: 0.25rem;
        padding: 0.375rem 0.75rem;
        font-size: 1rem;
        line-height: 1.5;
        color: #495057;
        background-color: #fff;
        background-clip: padding-box;
        transition: border-color 0.15s ease-in-out, box-shadow 0.15s ease-in-out;
    }

    .form-group {
        margin-bottom: 1rem;
    }

    .border-input {
        border: 1px solid #ccc;
        border-radius: 4px;
        padding: 0.5rem;
        width: 100%;
    }

    label {
        font-weight: bold;
        margin-bottom: 0.5rem;
        color: black;
    }

    .readonly-field {
        background-color: #ddd;
    }
</style>
