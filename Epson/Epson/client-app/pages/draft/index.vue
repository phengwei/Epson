<template>
  <div class="d-flex justify-content-center align-items-center vh-100" data-app="true" v-if="loggedInUser.roles.includes('Sales') || loggedInUser.roles.includes('Sales Section Head') || loggedInUser.roles.includes('Admin') || loggedInUser.roles.includes('Director')">
    <v-card class="mx-auto card-round" style="width: 90%; padding: 20px;">
      <v-card-title class="d-flex justify-content-between align-items-center">
        <v-toolbar flat>
          <v-toolbar-title><h2 class="blue-text big-bold">DRAFT MANAGEMENT</h2></v-toolbar-title>
          <v-spacer></v-spacer>
        </v-toolbar>

        <v-dialog v-model="dialog" max-width="500px">
          <v-card>
            <v-card-title>
              <span class="text-h5 blue-text big-bold">{{ formTitle }}</span>
            </v-card-title>
            <v-card-text>
              <div class="form-group">
                <label>Draft Name</label>
                <input v-model="editedItem.name" class="border-input" required></input>
              </div>
              <div class="form-group">
                <label>Comments</label>
                <input v-model="editedItem.comments" class="border-input"></input>
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
                      :items="filteredDrafts"
                      :options.sync="options"
                      :items-per-page="5"
                      :loading="loading"
                      class="elevation-1">
          <template v-slot:item.actions="{ item }">
            <v-tooltip bottom>
              <template v-slot:activator="{ on, attrs }">
                <v-icon small class="mr-2" v-bind="attrs" v-on="on" @click="editItem(item)">mdi-pencil</v-icon>
              </template>
              <span>Edit Draft</span>
            </v-tooltip>

            <v-tooltip bottom>
              <template v-slot:activator="{ on, attrs }">
                <v-icon small class="mr-2" v-bind="attrs" v-on="on" @click="deleteDraft(item)">mdi-delete</v-icon>
              </template>
              <span>Delete Draft</span>
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
    name: 'DraftTable',
    middleware: "auth",
    computed: {
      ...mapGetters(['isAuthenticated', 'loggedInUser']),
      formTitle() {
        return this.editedIndex === -1 ? 'CREATE DRAFT' : 'EDIT DRAFT';
      },
      filteredDrafts() {
        let filtered = this.drafts;
        if (this.search.trim() !== '') {
          const searchTerm = this.search.toLowerCase();
          filtered = filtered.filter(draft => {
            return draft.name.toLowerCase().includes(searchTerm);
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
          { text: 'Created On', value: 'createdOnUTC' },
          { text: 'Is Default', value: 'isDefault' },
          { text: 'Actions', value: 'actions', sortable: false },
        ],
        options: {},
        drafts: [],
        loading: true,
        search: '',
        editedIndex: -1,
        editedItem: {
          id: 0,
          name: '',
          comments: '',
          isDefault: false
        },
        defaultItem: {
          name: '',
          comments: '',
          isDefault: false
        },
      };
    },
    watch: {
      options: {
        handler() {
          this.getDrafts();
        },
        deep: true,
      },
      dialog(val) {
        val || this.close();
      },
    },
    created() {
      this.getDrafts();
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
      getDrafts() {
        this.loading = true;
        this.$axios.get(`${this.$config.restUrl}/api/request/getdrafts`).then(result => {
          this.drafts = result.data.data.map(draft => {
            return {
              ...draft,
              createdOnUTC: moment(draft.createdOnUTC).add(8, 'hours').format('DD MMM YY HH:mm')
            };
          }).sort((a, b) => moment(b.createdOnUTC, 'DD MMM YY HH:mm').valueOf() - moment(a.createdOnUTC, 'DD MMM YY HH:mm').valueOf());
          this.loading = false;
        });
      },
      async editItem(item) {
        console.log("awd", item);
        const result = await Swal.fire({
          title: 'Set Default Draft',
          icon: 'question',
          showCancelButton: true,
          confirmButtonColor: '#3085d6',
          cancelButtonColor: '#d33',
          confirmButtonText: 'Yes, set as default',
          cancelButtonText: 'Cancel'
        });

        if (result.isConfirmed) {
          try {
            await this.$axios.post(`${this.$config.restUrl}/api/request/defaultdraft?id=${item.id}`);

            Swal.fire('Updated default!', 'Success');
            this.getDrafts();
          } catch (error) {
            Swal.fire('Failed to update', error.response.data.message, 'error');
          }
        }
      },
      async deleteDraft(item) {
        const result = await Swal.fire({
          title: 'Are you sure?',
          text: "You won't be able to revert this!",
          icon: 'warning',
          showCancelButton: true,
          confirmButtonColor: '#3085d6',
          cancelButtonColor: '#d33',
          confirmButtonText: 'Yes, delete it!'
        });

        if (result.isConfirmed) {
          try {
            await this.$axios.post(`${this.$config.restUrl}/api/request/deletedraft?id=${item.id}`);
            Swal.fire('Deleted!', 'Your draft has been deleted.', 'success');
            this.getDrafts();
          } catch (error) {
            Swal.fire('Failed to delete', error.response.data.message, 'error');
          }
        }
      },
      close() {
        this.dialog = false;
        this.$nextTick(() => {
          this.editedItem = Object.assign({}, this.defaultItem);
          this.editedIndex = -1;
        });
      },
      async save() {
        if (!this.editedItem.name) {
          Swal.fire('Error!', 'Draft Name is required.', 'error');
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
              await this.$axios.post(`${this.$config.restUrl}/api/draft/updatedraft`, {
                id: this.editedItem.id,
                name: this.editedItem.name,
                comments: this.editedItem.comments
              });
              Swal.fire('Saved!', 'Your draft has been updated.', 'success');
            } else {
              await this.$axios.post(`${this.$config.restUrl}/api/draft/savedraft`, {
                name: this.editedItem.name,
                comments: this.editedItem.comments
              });
              Swal.fire('Saved!', 'Your draft has been created.', 'success');
            }
            this.getDrafts();
          } catch (error) {
            Swal.fire('Failed to save', error.response.data.message, 'error');
          } finally {
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
