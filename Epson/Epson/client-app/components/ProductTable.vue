<template>
  <v-data-table
      :headers="headers"
      :items="products"
      :options.sync="options"
      :items-per-page="5"
      :loading="loading"
      class="elevation-1"
    >
    <template v-slot:top>
      <v-toolbar
        flat
      >
        <v-toolbar-title>Products</v-toolbar-title>
        <v-divider
          class="mx-4"
          inset
          vertical
        ></v-divider>
        <v-spacer></v-spacer>
        <v-dialog
          v-model="dialog"
          max-width="500px"
        >
          <template v-slot:activator="{ on, attrs }">
            <v-btn
              color="primary"
              dark
              class="mb-2"
              v-bind="attrs"
              v-on="on"
            >
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
                <input v-model="editedItem.name" class="border-input" label="Product name"></input>
              </div>
              <div class="form-group">
                <label>Price</label>
                <input v-model="editedItem.price" class="border-input" label="Price"></input>
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
      <v-icon
        small
        class="mr-2"
        @click="editItem(item)"
      >
        mdi-pencil
      </v-icon>
      <v-icon
        small
        @click="deleteItem(item)"
      >
        mdi-delete
      </v-icon>
    </template>
    <template v-slot:no-data>
      <v-btn
        color="primary"
        @click="initialize"
      >
        Reset
      </v-btn>
    </template></v-data-table>
</template>

<script>
export default {
    name: 'ProductTable',
    data () {
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
    computed: {
      formTitle () {
        return this.editedIndex === -1 ? 'New Item' : 'Edit Item'
      },
      selectedCategoryList() {
        return this.categories.filter((category) => this.selectedCategories.includes(category.id));
      }
    },
    watch: {
      options: {
        handler () {
          this.getDataFromApi()
        },
        deep: true,
      },
      dialog (val) {
        val || this.close()
      },
      dialogDelete (val) {
        val || this.closeDelete()
      },
    },
    created() {
      // Initialize the options object with an empty array for each category
      this.getCategoryFromApi();
    },
    methods: {
      getDataFromApi () {
        this.loading = true
        this.$axios.get(`${this.$config.restUrl}/api/product/getproducts`).then(result => {
          this.products = result.data.data
          this.loading = false
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
      editItem (item) {
        this.editedIndex = this.products.indexOf(item)
        this.editedItem = Object.assign({}, item)
        this.dialog = true
      },

      deleteItem (item) {
        this.editedIndex = this.products.indexOf(item)
        this.editedItem = Object.assign({}, item)
        this.dialogDelete = true
      },
      checkboxChanged(selectedCategory) {
        
        console.log(this.selectedCategories);
       
      },
      async deleteItemConfirm () {
        const vm = this;
        // method for delete item
        try{
            await this.$axios.post(`${this.$config.restUrl}/api/product/deleteproduct?id=${this.editedItem.id}`).then(response => {
              console.log('res', response);
              this.getDataFromApi();
            }).catch(function (error) {
              console.log('vm error', error.response);
              vm.$swal('Failed to delete', error.response.data.errorList[0], 'error');
            })
          } catch (err){
            console.log('try', err);
          }
        this.closeDelete()
      },

      close () {
        this.dialog = false
        this.$nextTick(() => {
          this.editedItem = Object.assign({}, this.defaultItem)
          this.editedIndex = -1
        })
      },

      closeDelete () {
        this.dialogDelete = false
        this.$nextTick(() => {
          this.editedItem = Object.assign({}, this.defaultItem)
          this.editedIndex = -1
        })
      },

      async save() {
        console.log(this.editedIndex);
        const vm = this;
        if (this.editedIndex > -1) {
          // method for edit item
          try {
            console.log("test");
            await this.$axios.post(`${this.$config.restUrl}/api/product/editproduct`, {
              data: {
                id: this.editedItem.id,
                name: this.editedItem.name,
                price: this.editedItem.price,
                productcategories: this.selectedCategories.map(category => ({
                  categoryid: category.id,
                  productId: this.editedItem.id
                }))
              }
            }).then(response => {
              console.log('res', response);
              this.getDataFromApi();
            }).catch(err => {
              console.log(err);
              console.log(err.response);
              vm.$swal('Failed to add', err.response.data.message, 'error');
            })
          } catch (err){
            console.log(err);
          }
        } else {
          // method for add item
          try {
            await this.$axios.post(`${this.$config.restUrl}/api/product/addproduct`, {
              data: {
                name: this.editedItem.name,
                price: this.editedItem.price,
                productcategories: this.selectedCategories.map(category => ({
                  categoryid: category.id,
                  productId: this.editedItem.id
                }))
              }
            }).then(response => {
              console.log('res', response);
              this.getDataFromApi();
            }).catch(err => {
              console.log(err);
              console.log(err.response);
              vm.$swal('Failed to add', err.response.data.message, 'error');
            })
          } catch (err){
            console.log(err);
          }
        }
        this.loading = false;
        this.close();
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
