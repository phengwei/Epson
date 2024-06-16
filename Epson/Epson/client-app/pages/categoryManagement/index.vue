<template>
  <div class="d-flex justify-content-center align-items-center vh-100" data-app="true">
    <v-card class="mx-auto card-round" style="width: 90%; padding: 20px;">
      <v-toolbar flat>
        <v-toolbar-title><h2 class="blue-text big-bold">PRODUCT CATEGORIES</h2></v-toolbar-title>
        <v-spacer></v-spacer>
        <v-text-field v-model="search"
                      prepend-inner-icon="mdi-magnify"
                      placeholder="Search"
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
        <v-btn class="mr-5 blue-button" color="primary" dark @click="initializeUser, dialog = true">
          <v-icon left>mdi-plus</v-icon>
          ADD CATEGORY
        </v-btn>
      </v-tabs>

      <v-dialog v-model="dialog" max-width="500px">
        <v-card>
          <v-card-title>
            <span class="headline blue-text big-bold">{{ formTitle }}</span>
          </v-card-title>
          <v-card-text>
            <v-col cols="12">
              <div class="form-group">
                <label>Category Name</label>
                <input v-model="newCategory.name" class="border-input" required></input>
              </div>
              <div class="form-group">
                <label>Primary Fulfiller</label>
                <select v-model="newCategory.backupFulfiller1" class="border-input">
                  <option value="">None</option>
                  <option v-for="user in availableUsersForFulfiller1" :key="user.id" :value="user.id">
                    {{ user.userName }}
                  </option>
                </select>
              </div>
              <div class="form-group">
                <label>Secondary Fulfiller</label>
                <select v-model="newCategory.backupFulfiller2" class="border-input">
                  <option value="">None</option>
                  <option v-for="user in availableUsersForFulfiller2" :key="user.id" :value="user.id">
                    {{ user.userName }}
                  </option>
                </select>
              </div>
              <div class="form-group">
                <label>3rd Tier Escalation</label>
                <select v-model="newCategory.escalationFulfiller" class="border-input">
                  <option value="">None</option>
                  <option v-for="user in availableUsersForEscalationFulfiller" :key="user.id" :value="user.id">
                    {{ user.userName }}
                  </option>
                </select>
              </div>
            </v-col>
          </v-card-text>
          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="blue darken-1" text @click="dialog = false">Cancel</v-btn>
            <v-btn color="blue darken-1" text @click="saveCategoryConfirmation">Save</v-btn>
          </v-card-actions>
        </v-card>
      </v-dialog>

      <v-card-text>
        <v-data-table :headers="headers" :items="categories" class="elevation-1">
          <template v-slot:item.actions="{ item }">
            <v-icon small class="mr-2" @click="editCategory(item)">mdi-pencil</v-icon>
            <!--<v-icon small class="mr-2" @click="deleteCategoryConfirmation(item)">mdi-delete</v-icon>-->
          </template>
        </v-data-table>
      </v-card-text>

    </v-card>
  </div>
</template>

<script>
  export default {
    name: 'CategoryManagement',
    data() {
      return {
        dialog: false,
        editedIndex: -1,
        newCategory: {
          name: '',
          products: [],
          backupFulfiller1: '',
          backupFulfiller2: '',
          escalationFulfiller: ''
        },
        users: [],
        salesHeadUsers: [],
        backupFulfiller1: null,
        backupFulfiller2: null,
        escalationFulfiller: null,
        categories: [],
        headers: [
          { text: 'Category', value: 'name' },
          { text: 'Actions', value: 'actions', sortable: false },
        ],
        isEditing: false,
        tab: null,
      };
    },
    computed: {
      formTitle() {
        return this.editedIndex === -1 ? 'ADD CATEGORY' : 'EDIT CATEGORY'
      },
      availableUsersForFulfiller1() {
        return this.users.filter(user => user.id !== this.newCategory.backupFulfiller2 &&
          user.id !== this.newCategory.escalationFulfiller);
      },
      availableUsersForFulfiller2() {
        return this.users.filter(user => user.id !== this.newCategory.backupFulfiller1 &&
          user.id !== this.newCategory.escalationFulfiller);
      },
      availableUsersForEscalationFulfiller() {
        return this.users.filter(user => user.id !== this.newCategory.backupFulfiller1 &&
          user.id !== this.newCategory.backupFulfiller2);
      },
    },
    created() {
      this.getCategories();
      this.getUsers();
    },
    watch: {
      dialog(newVal) {
        if (!newVal) {
          this.initializeCategory();
        }
      }
    },
    methods: {
      getUsers() {
        this.$axios.get(`${this.$config.restUrl}/api/customer/getallfulfillers`)
          .then(response => {
            this.users = response.data.data;
          })
          .catch(error => {
            console.error('Error fetching users:', error);
          });
      },
      saveCategoryConfirmation() {
        if (this.newCategory.name.trim() === '') {
          this.$swal({
            title: 'Validation Error',
            text: "Category name cannot be empty!",
            icon: 'error',
            confirmButtonColor: '#3085d6',
          });
          return;
        }

        this.$swal({
          title: 'Are you sure?',
          text: "You are about to save the category data!",
          icon: 'warning',
          showCancelButton: true,
          confirmButtonColor: '#3085d6',
          cancelButtonColor: '#d33',
          confirmButtonText: 'Yes, save it!'
        }).then((result) => {
          if (result.isConfirmed) {
            this.saveCategory();
          }
        })
      },
      initializeCategory() {
        this.isEditing = false;
        this.newCategory = {
          name: '',
          backupFulfiller1: '',
          backupFulfiller2: '',
          escalationFulfiller: ''
        };
        this.editedIndex = -1;
      },
      getCategories() {
        this.$axios.get(`${this.$config.restUrl}/api/category/getcategories`)
          .then(response => {
            this.categories = response.data.data;
          })
          .catch(error => {
            console.error('Error fetching categories:', error);
          });
      },
      addNewCategory() {
        this.$axios.post(`${this.$config.restUrl}/api/category/addcategory`, {
          data: {
            name: this.newCategory.name,
            backupFulfiller1: this.newCategory.backupFulfiller1,
            backupFulfiller2: this.newCategory.backupFulfiller2,
            escalationFulfiller: this.newCategory.escalationFulfiller
          }
        }).then(response => {
          this.dialog = false;
          this.$swal('Success', 'Category added successfully.', 'success').then(() => {
            location.reload();
          });
        }).catch(error => {
          console.error('Error adding category:', error);
          this.$swal('Failed to add category', error.response.data.message, 'error');
        });
      },
      editCategory(item) {
        this.isEditing = true;
        this.editedIndex = this.categories.indexOf(item)
        this.newCategory = Object.assign({}, item)
        this.newCategory.name = item.name;
        this.newCategory.products = [];
        this.dialog = true;
      },
      saveCategory() {
        if (this.editedIndex > -1) {
          this.$axios.post(`${this.$config.restUrl}/api/category/editCategory`, {
            data: {
              id: this.newCategory.id,
              name: this.newCategory.name,
              backupFulfiller1: this.newCategory.backupFulfiller1,
              backupFulfiller2: this.newCategory.backupFulfiller2,
              escalationFulfiller: this.newCategory.escalationFulfiller
            }
          }).then(response => {
            Object.assign(this.categories[this.editedIndex], response.data);
            this.dialog = false;
            this.$swal('Success', 'Category data saved successfully.', 'success').then(() => {
              location.reload();
            });
          }).catch(error => {
            console.error('Error editing category:', error);
            this.$swal('Failed to edit category', error.response.data.message, 'error');
          });
        } else {
          this.addNewCategory();
        }
      },
      deleteCategory(id) {
        this.$axios.delete(`${this.$config.restUrl}/api/category/deletecategory?id=${id}`)
          .then(response => {
            this.categories.splice(id, 1);
            this.$swal('Success', 'Category deleted successfully.', 'success').then(() => {
              location.reload();
            });
          }).catch(error => {
            console.error('Error deleting category:', error);
            this.$swal('Failed to delete category', error.response.data.message, 'error');
          });
      },
      deleteCategoryConfirmation(item) {
        this.$swal({
          title: 'Are you sure?',
          text: "You won't be able to revert this!",
          icon: 'warning',
          showCancelButton: true,
          confirmButtonColor: '#3085d6',
          cancelButtonColor: '#d33',
          confirmButtonText: 'Yes, delete it!'
        }).then((result) => {
          if (result.isConfirmed) {
            this.deleteCategory(item.id);
          }
        })
      }
    },
  };
</script>

<style scoped>
  @import '~@/../wwwroot/css/general-table.css';
</style>
