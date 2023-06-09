<template>
  <div class="d-flex justify-content-center align-items-center vh-100" data-app="true" v-if="loggedInUser.roles.includes('Product')">
    <v-card class="mx-auto" style="width: 90%">
      <v-card-title class="d-flex justify-content-between align-items-center">
      </v-card-title>
      <v-card-text>
        <v-data-table :headers="headers"
                      :items="products"
                      :options.sync="options"
                      :items-per-page="5"
                      :loading="loading"
                      class="elevation-1">
          <template v-slot:top>
            <v-toolbar flat>
              <v-toolbar-title>Products</v-toolbar-title>
              <v-divider class="mx-4"
                         inset
                         vertical></v-divider>
              <v-spacer></v-spacer>
              <v-dialog v-model="dialog"
                        max-width="500px">
                <template v-slot:activator="{ on, attrs }">
                  <v-btn color="primary"
                         dark
                         class="mb-2"
                         v-bind="attrs"
                         v-on="on">
                    New Product
                  </v-btn>
                </template>

                <v-card>
                  <v-card-title>
                    <span class="text-h5">{{ formTitle }}</span>
                  </v-card-title>
                  <v-card-text>
                    <label>Product Category</label>
                    <div v-for="category in categories" :key="category.id">
                      <style>
                      </style>
                      <div class="blue-checkbox">
                        <input type="checkbox" v-model="selectedCategories" :value="category" @change="checkboxChanged(category)">
                        <label class="category-name">{{ category.name }}</label>
                      </div>
                    </div>
                    <div class="form-group">
                      <label>Product Name</label>
                      <input v-model="editedItem.name" class="border-input" label="Product name" required></input>
                    </div>
                    <div class="form-group">
                      <label>Price</label>
                      <input type="number" v-model="editedItem.price" class="border-input" label="Price" required></input>
                    </div>

                    <v-card-actions>
                      <v-spacer></v-spacer>
                      <v-btn color="blue darken-1" text @click="close">Cancel</v-btn>
                      <v-btn color="blue darken-1" text @click="save">Save</v-btn>
                    </v-card-actions>
                  </v-card-text>
                </v-card>
              </v-dialog>
              <v-dialog v-model="dialogDelete" max-width="500px">
                <v-card>
                  <v-card-title class="text-h5">Are you sure you want to delete this item?</v-card-title>
                  <v-card-actions>
                    <v-spacer></v-spacer>
                    <v-btn color="blue darken-1" text @click="closeDelete">Cancel</v-btn>
                    <v-btn color="blue darken-1" text @click="deleteItemConfirm">OK</v-btn>
                    <v-spacer></v-spacer>
                  </v-card-actions>
                </v-card>
              </v-dialog>
            </v-toolbar>
          </template>
          <template v-slot:item.actions="{ item }">
            <v-icon small
                    class="mr-2"
                    @click="editItem(item)">
              mdi-pencil
            </v-icon>
            <v-icon small
                    @click="deleteItem(item)">
              mdi-delete
            </v-icon>
          </template>
        </v-data-table>

      </v-card-text>
    </v-card>
  </div>
</template>

<script>
  import { mapGetters } from 'vuex';
  import Swal from 'sweetalert2'

  export default {
    name: 'ProductTable',
    middleware: "auth",
    computed: {
      ...mapGetters(['isAuthenticated', 'loggedInUser']),
      formTitle() {
        return this.editedIndex === -1 ? 'New Product' : 'Edit Product'
      },
      selectedCategoryList() {
        return this.categories.filter((category) => this.selectedCategories.includes(category.id));
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
          { text: 'Price', value: 'price' },
          { text: 'Actions', value: 'actions', sortable: false },
        ],
        options: {},
        products: [],
        categories: [],
        loading: true,
        selectedCategories: [],
        totalProducts: 0,
        editedIndex: -1,
        editedItem: {
          id: 0,
          name: '',
          price: 0,
        },
        defaultItem: {
          name: '',
          price: 0,
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
    },
    methods: {
      getProducts() {
        this.loading = true
        this.$axios.get(`${this.$config.restUrl}/api/product/getproducts`).then(result => {
          this.products = result.data.data
          this.loading = false
          console.log("products", this.products);
        })
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

      deleteItem(item) {
        this.editedIndex = this.products.indexOf(item)
        this.editedItem = Object.assign({}, item)
        this.dialogDelete = true
      },
      checkboxChanged(selectedCategory) {

        console.log(this.selectedCategories);

      },
      async deleteItemConfirm() {
        const vm = this;
        try {
          await this.$axios.post(`${this.$config.restUrl}/api/product/deleteproduct?id=${this.editedItem.id}`).then(response => {
            console.log('res', response);
            this.getProducts();
          }).catch(function (error) {
            console.log('vm error', error.response);
            vm.$swal('Failed to delete', error.response.data.errorList[0], 'error');
          })
        } catch (err) {
          console.log('try', err);
        }
        this.closeDelete()
      },

      close() {
        this.dialog = false
        this.$nextTick(() => {
          this.editedItem = Object.assign({}, this.defaultItem)
          this.editedIndex = -1
        })
      },

      closeDelete() {
        this.dialogDelete = false
        this.$nextTick(() => {
          this.editedItem = Object.assign({}, this.defaultItem)
          this.editedIndex = -1
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
            'Price is required.',
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
            let response;
            if (this.editedIndex > -1) {
              response = await this.$axios.post(`${this.$config.restUrl}/api/product/editproduct`, {
                data: {
                  id: this.editedItem.id,
                  name: this.editedItem.name,
                  price: this.editedItem.price,
                  productcategories: this.selectedCategories.map(category => ({
                    categoryid: category.id,
                    productId: this.editedItem.id
                  }))
                }
              });

              console.log('res', response);
              this.getProducts();
              Swal.fire(
                'Saved!',
                'Your product has been updated.',
                'success'
              );
            } else {
              response = await this.$axios.post(`${this.$config.restUrl}/api/product/addproduct`, {
                data: {
                  name: this.editedItem.name,
                  price: this.editedItem.price,
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
<style>
  .form-group {
    margin-bottom: 1rem;
    display: flex;
    justify-content: center;
    flex-direction: column;
  }

  label {
    font-weight: bold;
    margin-bottom: 0.5rem;
    color: black;
  }

  .border-input {
    border: 1px solid #ccc;
    border-radius: 4px;
    padding: 0.5rem;
    width: 100%;
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
</style>
