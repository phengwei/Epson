<template>
  <div class="d-flex justify-content-center align-items-center vh-100" data-app="true">
    <v-card class="mx-auto" style="width: 90%">
      <v-card-title>
        Manage Users
        <v-spacer></v-spacer>
        <v-btn color="primary" dark @click="initializeUser, dialog = true">New User</v-btn>
      </v-card-title>

      <v-dialog v-model="dialog" max-width="500px">
        <v-card>
          <v-card-title>
            <span class="headline">{{ formTitle }}</span>
          </v-card-title>
          <v-card-text>
            <v-col cols="12">
              <div class="form-group">
                <label>Roles</label>
                <div v-for="role in roles" :key="role.Id">
                  <input type="checkbox" :value="role" :checked="newUser.selectedRoles.includes(role)" @change="toggleRole(role)">
                  <label class="role-name">{{ role.name }}</label>
                </div>
              </div>
              <div class="form-group">
                <label>Team</label>
                <select v-model="newUser.teamId" class="border-input">
                  <option v-for="team in teams" :key="team.id" :value="team.id">
                    {{ team.name }}
                  </option>
                </select>
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
            <v-btn color="blue darken-1" text @click="saveUserConfirmation">Save</v-btn>
          </v-card-actions>
        </v-card>
      </v-dialog>

      <v-dialog v-model="dialogPassword" max-width="500px">
        <v-card>
          <v-card-title>
            <span class="headline">Reset Password</span>
          </v-card-title>
          <v-card-text>
            <v-col cols="12">
              <div class="form-group">
                <label>Password</label>
                <input v-model="password" class="border-input" required></input>
              </div>
            </v-col>
          </v-card-text>
          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="blue darken-1" text @click="dialogPassword = false">Cancel</v-btn>
            <v-btn color="blue darken-1" text @click="changePasswordConfirmation">Save</v-btn>
          </v-card-actions>
        </v-card>
      </v-dialog>

      <v-tabs v-model="tab" background-color="white">
        <v-tab v-for="(item, index) in tabItems" :key="index">{{ item }}</v-tab>
      </v-tabs>

      <v-card-text>
        <v-tabs-items v-model="tab">
          <v-tab-item v-for="(item, index) in tabItems" :key="index">
            <v-data-table :headers="headers" :items="filteredUsers(item)" class="elevation-1">
              <template v-slot:item.actions="{ item }">
                <v-icon small class="mr-2" @click="editUser(item)">mdi-pencil</v-icon>
                <v-icon small class="mr-2" @click="deleteUserConfirmation(item)">mdi-delete</v-icon>
                <v-icon small class="mr-2" @click="changePassword(item)">mdi-lock-reset</v-icon>
                <v-icon small v-if="item.lockoutEnd != null" class="mr-2" @click="reactivateAccountConfirmation(item)">mdi-account-reactivate</v-icon>
              </template>
            </v-data-table>
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
        dialogPassword: false,
        editedIndex: -1,
        newUser: {
          userName: '',
          email: '',
          phone: '',
          password: '',
          selectedRoles: []
        },
        password: '',
        roles: [],
        headers: [
          { text: 'User Name', value: 'userName' },
          { text: 'Email', value: 'email' },
          { text: 'Team', value: 'teams' },
          { text: 'Actions', value: 'actions', sortable: false },
        ],
        tabItems: ['Sales', 'Product', 'Admin', 'Coverplus', 'Sales Section Head'],
        tab: null,
        users: [],
        teams: [],
        isEditing: false,
      };
    },
    computed: {
      formTitle() {
        return this.editedIndex === -1 ? 'New User' : 'Edit User'
      },
    },
    created() {
      this.getUsers();
      this.getAllRoles();
      this.getTeams();
    },
    watch: {
      dialog(newVal) {
        if (!newVal) {
          this.initializeUser();
        }
      }
    },
    methods: {
      getUserNameValidationError() {
        if (this.newUser.userName.includes(' ')) {
          return 'Username should not contain spaces.';
        }
        return null;
      },
      saveUserConfirmation() {
        this.$swal({
          title: 'Are you sure?',
          text: "You are about to save the user data!",
          icon: 'warning',
          showCancelButton: true,
          confirmButtonColor: '#3085d6',
          cancelButtonColor: '#d33',
          confirmButtonText: 'Yes, save it!'
        }).then((result) => {
          if (result.isConfirmed) {
            this.saveUser();
          }
        })
      },
      changePasswordConfirmation() {
        if (!this.validatePassword(this.password)) {
          this.$swal('Invalid Password', 'Password must have at least one uppercase letter, one lowercase letter, one digit, and one non-numeric character.', 'error');
          return;
        }
        this.$swal({
          title: 'Are you sure?',
          text: "You are about to change the user password!",
          icon: 'warning',
          showCancelButton: true,
          confirmButtonColor: '#3085d6',
          cancelButtonColor: '#d33',
          confirmButtonText: 'Yes, change it!'
        }).then((result) => {
          if (result.isConfirmed) {
            this.saveUserPassword();
          }
        })
      },
      initializeUser() {
        this.isEditing = false;
        this.newUser = {
          userName: '',
          email: '',
          phone: '',
          password: '',
          selectedRoles: []
        };
        this.editedIndex = -1;
      },
      toggleRole(role) {
        const index = this.newUser.selectedRoles.indexOf(role);

        if (index > -1) {
          this.newUser.selectedRoles.splice(index, 1);
        } else {
          this.newUser.selectedRoles.push(role);
        }
      },
      getUsers() {
        this.$axios.get(`${this.$config.restUrl}/api/customer/getallusers`)
          .then(response => {
            this.users = response.data.data;
          })
          .catch(error => {
            console.error('Error fetching users:', error);
          });
      },
      getTeams() {
        this.$axios.get(`${this.$config.restUrl}/api/customer/getavailableteams`)
          .then(response => {
            this.teams = response.data.data;
          })
          .catch(error => {
            console.error('Error fetching teams:', error);
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
        const errorMessage = this.getUserNameValidationError();
        if (errorMessage) {
          this.$swal('Validation Error', errorMessage, 'error');
          return;  
        }
        this.$axios.post(`${this.$config.restUrl}/api/customer/addnewuser`, {
          data: {
            userName: this.newUser.userName,
            email: this.newUser.email,
            phone: this.newUser.phone,
            password: 'Abcde123.',
            roles: this.newUser.selectedRoles.map(role => role.name),
            teamId: this.newUser.teamId
          }
        }).then(response => {
          this.users.push(response.data);
          this.dialog = false;
          this.$swal('Success', 'User added successfully.', 'success').then(() => {
            location.reload();
          });
        }).catch(error => {
          console.error('Error adding user:', error);
          this.$swal('Failed to add user', error.response.data.message, 'error');
        });
      },
      filteredUsers(tab) {
        return this.users.filter(user => Array.isArray(user.roles) && user.roles.includes(tab));
      },
      editUser(item) {
        this.isEditing = true;
        this.editedIndex = this.users.indexOf(item)
        this.newUser = Object.assign({}, item)
        this.newUser.selectedRoles = item.roles.map(roleName => {
          return this.roles.find(role => role.name === roleName)
        })
        this.newUser.teamId = item.teamId;
        this.dialog = true
      },
      saveUser() {
        if (this.editedIndex > -1) {
          this.$axios.post(`${this.$config.restUrl}/api/customer/edituser`, {
            data: {
              id: this.newUser.id,
              userName: this.newUser.userName,
              email: this.newUser.email,
              phone: this.newUser.phone,
              password: this.newUser.password,
              roles: this.newUser.selectedRoles.map(role => role.name),
              teamId: this.newUser.teamId,
            }
          }).then(response => {
            Object.assign(this.users[this.editedIndex], response.data);
            this.dialog = false;
            this.$swal('Success', 'User data saved successfully.', 'success').then(() => {
              location.reload();
            });
          }).catch(error => {
            console.error('Error editing user:', error);
            this.$swal('Failed to edit user', error.response.data.message, 'error');
          });
        } else {
          this.addNewUser();
        }
      },
      changePassword(item) {
        this.isEditing = true;
        this.editedIndex = this.users.indexOf(item)
        this.newUser = Object.assign({}, item)
        this.newUser.selectedRoles = item.roles.map(roleName => {
          return this.roles.find(role => role.name === roleName)
        })
        this.newUser.teamId = item.teamId;
        this.dialogPassword = true
      },
      saveUserPassword() {
        if (this.editedIndex > -1) {
          this.$axios.post(`${this.$config.restUrl}/api/customer/adminchangepassword?newPassword=${this.password}`
          ).then(response => {
            Object.assign(this.users[this.editedIndex], response.data);
            this.dialogPassword = false;
            this.$swal('Success', 'User password changed successfully.', 'success').then(() => {
              location.reload();
            });
          }).catch(error => {
            console.error('Error changing user password:', error);
            this.$swal('Failed to change user password', error.response.data.message, 'error');
          });
        } 
      },
      validatePassword(password) {
        const hasUppercase = /[A-Z]/.test(password);
        const hasLowercase = /[a-z]/.test(password);
        const hasNumeric = /\d/.test(password);
        const hasNonNumeric = /\W|_/.test(password);
        const hasMinimumLength = password.length >= 8;

        return hasUppercase && hasLowercase && hasNumeric && hasNonNumeric && hasMinimumLength;
      },
      deleteUser(index) {
        this.$axios.delete(`${this.$config.restUrl}/api/customer/deleteuser?userId=${this.users[index].id}`)
          .then(response => {
            this.users.splice(index, 1);
            this.$swal('Success', 'User deleted successfully.', 'success').then(() => {
              location.reload();
            });
          }).catch(error => {
            console.error('Error deleting user:', error);
            this.$swal('Failed to delete user', error.response.data.message, 'error');
          });
      },
      deleteUserConfirmation(item) {
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
            this.deleteUser(this.users.indexOf(item));
          }
        })
      },
      reactivateAccountConfirmation(item) {
        this.$swal({
          title: 'Are you sure?',
          text: "Unlock account!",
          icon: 'warning',
          showCancelButton: true,
          confirmButtonColor: '#3085d6',
          cancelButtonColor: '#d33',
          confirmButtonText: 'Yes, unlock it!'
        }).then((result) => {
          if (result.isConfirmed) {
            this.unlockAccount(this.users.indexOf(item));
          }
        })
      },
      unlockAccount(index) {
        this.$axios.post(`${this.$config.restUrl}/api/customer/unlockAccount?userId=${this.users[index].id}`)
          .then(response => {
            this.users.splice(index, 1);
            this.$swal('Success', 'User account unlocked successfully.', 'success').then(() => {
              location.reload();
            });
          }).catch(error => {
            console.error('Error unlocking account:', error);
            this.$swal('Failed to unlock account', error.response.data.message, 'error');
          });
      }
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
    border: 1px solid #ccc;
    border-radius: 3px;
    margin-right: 0.5rem;
    width: 1.2em;
    height: 1.2em;
  }
</style>
