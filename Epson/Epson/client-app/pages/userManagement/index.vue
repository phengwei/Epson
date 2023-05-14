<template>
  <div class="d-flex justify-content-center align-items-center vh-100" data-app="true">
    <v-card class="mx-auto" style="width: 90%">
      <v-card-title>
        Manage Users
        <v-spacer></v-spacer>
        <v-btn color="primary" dark @click="dialog = true">New User</v-btn>
      </v-card-title>

      <v-dialog v-model="dialog" max-width="500px">
        <v-card>
          <v-card-title>
            <span class="headline">Add New User</span>
          </v-card-title>
          <v-card-text>
            <v-col cols="12">
              <div class="form-group">
                <label>Roles</label>
                <div v-for="role in roles" :key="role.Id">
                  <input type="checkbox" :value="role" v-model="newUser.selectedRoles" class="role-checkbox">
                  <label class="role-name">{{ role.name }}</label>
                </div>
              </div>
              <div class="form-group">
                <label>User Name</label>
                <input v-model="newUser.userName" class="border-input" required></input>
              </div>
              <div class="form-group">
                <label>Email</label>
                <input v-model="newUser.email" class="border-input" required></input>
              </div>
              <div class="form-group">
                <label>Phone Number</label>
                <input v-model="newUser.phone" class="border-input"></input>
              </div>
            </v-col>
          </v-card-text>
          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="blue darken-1" text @click="dialog = false">Cancel</v-btn>
            <v-btn color="blue darken-1" text @click="addNewUser">Save</v-btn>
          </v-card-actions>
        </v-card>
      </v-dialog>

      <v-tabs v-model="tab" background-color="white">
        <v-tab v-for="(item, index) in tabItems" :key="index">{{ item }}</v-tab>
      </v-tabs>

      <v-card-text>
        <v-tabs-items v-model="tab">
          <v-tab-item v-for="(item, index) in tabItems" :key="index">
            <v-data-table :headers="headers" :items="filteredUsers(item)" class="elevation-1"></v-data-table>
          </v-tab-item>
        </v-tabs-items>
      </v-card-text>

    </v-card>
  </div>
</template>

<script>
  export default {
    name: 'UserManagement',
    data() {
      return {
        dialog: false,
        newUser: {
          userName: '',
          email: '',
          phone: '',
          password: '',
          selectedRoles: []
        },
        roles: [],
        headers: [
          { text: 'User Name', value: 'userName' },
          { text: 'Email', value: 'email' },
        ],
        tabItems: ['Sales', 'Product', 'Admin'],
        tab: null,
        users: [],
      };
    },
    created() {
      this.getUsers();
      this.getAllRoles();
    },
    methods: {
      getUsers() {
        this.$axios.get(`${this.$config.restUrl}/api/customer/getallusers`)
          .then(response => {
            this.users = response.data.data;
          })
          .catch(error => {
            console.error('Error fetching users:', error);
          });
      },
      getAllRoles() {
        this.$axios.get(`${this.$config.restUrl}/api/customer/getallroles`)
          .then(response => {
            this.roles = response.data.data;
          })
          .catch(error => {
            console.error('Error fetching roles:', error);
          });
      },
      addNewUser() {
        this.$axios.post(`${this.$config.restUrl}/api/customer/addnewuser`, {
          data: {
            userName: this.newUser.userName,
            email: this.newUser.email,
            phone: this.newUser.phone,
            password: 'Abcde123.',
            roles: this.newUser.selectedRoles.map(role => role.name)
          }
        }).then(response => {
          this.users.push(response.data);
          this.dialog = false;
          this.newUser = {
            userName: '',
            email: '',
          };
        }).catch(error => {
          console.error('Error adding user:', error);
          this.$swal('Failed to add user', error.response.data.message, 'error');
        });
      },
      filteredUsers(tab) {
        return this.users.filter(user => user.roles.includes(tab));
      },
    },
  };
</script>

<style scoped>
  .vh-100 {
    height: 100vh;
  }

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

  .role-checkbox {
    border: 1px solid #ccc; /* Add border style */
    border-radius: 3px;
    margin-right: 0.5rem;
    width: 1.2em;
    height: 1.2em;
  }
</style>
