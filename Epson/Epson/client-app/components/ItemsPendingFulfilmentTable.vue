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
          <v-toolbar-title>Items Pending For Fulfilment</v-toolbar-title>
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
                <v-container>
                  <v-row>
                    <v-col
                      cols="12"
                      sm="6"
                      md="4"
                    >
                      <v-text-field
                        v-model="editedItem.name"
                        label="Product name"
                      ></v-text-field>
                    </v-col>
                    <v-col
                      cols="12"
                      sm="6"
                      md="4"
                    >
                      <v-text-field
                        v-model="editedItem.price"
                        label="Price"
                      ></v-text-field>
                    </v-col>
                    
                  </v-row>
                </v-container>
              </v-card-text>
  
              <v-card-actions>
                <v-spacer></v-spacer>
                <v-btn
                  color="blue darken-1"
                  text
                  @click="close"
                >
                  Cancel
                </v-btn>
                <v-btn
                  color="blue darken-1"
                  text
                  @click="save"
                >
                  Save
                </v-btn>
              </v-card-actions>
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
      name: 'ItemsPendingFulfilmentTable',
      data () {
        return {
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
          loading: true,
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
  
      methods: {
        getDataFromApi () {
          this.loading = true
          this.$axios.get(`${this.$config.restUrl}/api/product/getproducts`).then(result => {
            console.log('result', result);
            this.products = result.data.data
            this.totalProducts = result.data.data.length
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
  
        async deleteItemConfirm () {
          const vm = this;
          // method for delete item
          try{
              await this.$axios.post(`${this.$config.restUrl}/api/product/deleteproduct?id=${this.editedItem.id}`).then(response => {
                console.log('res', response);
                this.getDataFromApi();
              }).catch(err => {
                console.log(err);
                console.log(err.response);
                vm.$swal('Failed to delete', err.response.data.message, 'error');
              })
            } catch (err){
              console.log(err);
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
  
        async save () {
          const vm = this;
          if (this.editedIndex > -1) {
            // method for edit item
            try{
              await this.$axios.post(`${this.$config.restUrl}/api/product/editproduct`, {
                data: {
                  id: this.editedItem.id,
                  name: this.editedItem.name,
                  price: this.editedItem.price
                }
              }).then(response => {
                console.log('res', response);
                this.getDataFromApi();
              }).catch(err => {
                console.log(err);
                console.log(err.response);
                vm.$swal('Failed to edit', err.response.data.message, 'error');
              })
            } catch (err){
              console.log(err);
            }
          } else {
            // method for add item
            try{
              await this.$axios.post(`${this.$config.restUrl}/api/product/addproduct`, {
                data: {
                  name: this.editedItem.name,
                  price: this.editedItem.price
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
          this.close()
        },
  
      },
  }
  </script>
  
  <style>
  
  </style>