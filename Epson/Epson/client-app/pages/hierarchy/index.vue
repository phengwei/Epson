<template>
  <div class="d-flex justify-content-center align-items-center vh-100" data-app="true">
    <v-card class="mx-auto card-round" style="width: 90%; padding: 20px;">
      <v-toolbar flat>
        <v-toolbar-title><h2 class="blue-text big-bold">TEAM HIERARCHY</h2></v-toolbar-title>
        <v-spacer></v-spacer>
      </v-toolbar>

      <v-tabs class="mt-4" v-model="tab" background-color="white">
        <v-tab v-for="(item, index) in tabItems" :key="index" :class="{'blue-text--active': tab === index}">
          {{ item }}
        </v-tab>
        <v-spacer></v-spacer>
        <v-btn class="mr-5 blue-button" color="primary" dark @click="initializeHierarchy, dialog = true">
          <v-icon left>mdi-plus</v-icon>
          ADD HIERARCHY
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
                <label>Requesting Team</label>
                <select v-model="newHierarchy.requestingTeam" class="border-input" required>
                  <option value="">Select Team</option>
                  <option v-for="team in availableTeams" :key="team.id" :value="team.name">{{ team.name }}</option>
                </select>
              </div>
              <div class="form-group">
                <label>Approver Team</label>
                <select v-model="newHierarchy.approverTeam" class="border-input" required>
                  <option value="">Select Team</option>
                  <option v-for="team in availableTeams" :key="team.id" :value="team.name">{{ team.name }}</option>
                </select>
              </div>
              <div class="form-group">
                <label>Approval Level</label>
                <select v-model="newHierarchy.approvalLevel" @change="onApprovalLevelChange" class="border-input" required>
                  <option :value="1">1 - Non-Sales Head</option>
                  <option :value="2">2 - Sales Head</option>
                  <option :value="10">10 - Product Division Head</option>
                </select>
              </div>
              <div class="form-group">
                <label>Tagged Approver</label>
                <select v-model="newHierarchy.emailRecipient" class="border-input" required>
                  <option v-for="user in availableApprovers" :key="user.id" :value="user.email">{{ user.email }}</option>
                </select>
              </div>
            </v-col>
          </v-card-text>
          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="blue darken-1" text @click="dialog = false">Cancel</v-btn>
            <v-btn color="blue darken-1" text @click="saveHierarchyConfirmation">Save</v-btn>
          </v-card-actions>
        </v-card>
      </v-dialog>

      <v-card-text>
        <v-data-table :headers="headers" :items="hierarchies" class="elevation-1">
          <template v-slot:item.actions="{ item }">
            <v-tooltip bottom>
              <template v-slot:activator="{ on, attrs }">
                <v-icon small class="mr-2" v-bind="attrs" v-on="on" @click="editHierarchy(item)">mdi-pencil</v-icon>
              </template>
              <span>Edit Hierarchy</span>
            </v-tooltip>
            <v-tooltip bottom>
              <template v-slot:activator="{ on, attrs }">
                <v-icon small class="mr-2" v-bind="attrs" v-on="on" @click="deleteHierarchy(item.id)">mdi-delete</v-icon>
              </template>
              <span>Delete Hierarchy</span>
            </v-tooltip>
          </template>
        </v-data-table>
      </v-card-text>

    </v-card>
  </div>
</template>

<script>
  export default {
    name: 'HierarchyManagement',
    data() {
      return {
        dialog: false,
        editedIndex: -1,
        newHierarchy: {
          requestingTeam: '',
          approverTeam: '',
          approvalLevel: null,
          emailRecipient: ''
        },
        availableTeams: [],
        availableApprovers: [],
        hierarchies: [],
        headers: [
          { text: 'Requesting Team', value: 'requestingTeam' },
          { text: 'Approver Team', value: 'approverTeam' },
          { text: 'Approval Level', value: 'approvalLevel' },
          { text: 'Tagged Approver', value: 'emailRecipient' },
          { text: 'Actions', value: 'actions', sortable: false },
        ],
        search: '',
        tab: null,
        tabItems: ['Team Hierarchy'],
      };
    },
    computed: {
      formTitle() {
        return this.editedIndex === -1 ? 'ADD HIERARCHY' : 'EDIT HIERARCHY';
      }
    },
    created() {
      this.getTeams();
      this.getHierarchies();
    },
    watch: {
      dialog(newVal) {
        if (!newVal) {
          this.initializeHierarchy();
        }
      }
    },
    methods: {
      getTeams() {
        this.$axios.get(`${this.$config.restUrl}/api/customer/getavailableteams`)
          .then(response => {
            this.availableTeams = response.data.data;
          })
          .catch(error => {
            console.error('Error fetching teams:', error);
          });
      },
      getHierarchies() {
        this.$axios.get(`${this.$config.restUrl}/api/customer/getHierarchy`)
          .then(response => {
            this.hierarchies = response.data;
          })
          .catch(error => {
            console.error('Error fetching hierarchies:', error);
          });
      },
      onApprovalLevelChange() {
        const role = this.getRoleByApprovalLevel(this.newHierarchy.approvalLevel);
        this.newHierarchy.emailRecipient = '';  
        if (role) {
          this.getApproversByRole(role);
        }
      },
      getRoleByApprovalLevel(approvalLevel) {
        switch (approvalLevel) {
          case 1:
            return 'Sales Section Head';
          case 2:
            return 'Sales Section Head';
          case 10:
            return 'Product';
          default:
            return null;
        }
      },
      getApproversByRole(role) {
        return this.$axios.get(`${this.$config.restUrl}/api/customer/getusersbyrole`, { params: { role } })
          .then(response => {
            this.availableApprovers = response.data;
          })
          .catch(error => {
            console.error(`Error fetching users for role ${role}:`, error);
          });
      },
      saveHierarchyConfirmation() {
        if (!this.newHierarchy.requestingTeam || !this.newHierarchy.approverTeam || this.newHierarchy.approvalLevel === null || !this.newHierarchy.emailRecipient) {
          this.$swal({
            title: 'Validation Error',
            text: "All fields are required!",
            icon: 'error',
            confirmButtonColor: '#3085d6',
          });
          return;
        }

        this.$swal({
          title: 'Are you sure?',
          text: "You are about to save the hierarchy!",
          icon: 'warning',
          showCancelButton: true,
          confirmButtonColor: '#3085d6',
          cancelButtonColor: '#d33',
          confirmButtonText: 'Yes, save it!'
        }).then((result) => {
          if (result.isConfirmed) {
            if (this.newHierarchy.id && this.newHierarchy.id > 0) {
              this.updateHierarchy(); 
            } else {
              this.saveHierarchy();  
            }
          }
        })
      },
      initializeHierarchy() {
        this.newHierarchy = {
          requestingTeam: '',
          approverTeam: '',
          approvalLevel: null,
          emailRecipient: ''
        };
        this.editedIndex = -1;
        this.availableApprovers = [];
      },
      saveHierarchy() {
        const isSalesHead = this.newHierarchy.approvalLevel === 2;
        this.$axios.post(`${this.$config.restUrl}/api/customer/addhierarchy`, {
          data: {
            requestingTeam: this.newHierarchy.requestingTeam,
            approverTeam: this.newHierarchy.approverTeam,
            isSalesHead,
            approvalLevel: this.newHierarchy.approvalLevel,
            emailRecipient: this.newHierarchy.emailRecipient
          }
        }).then(response => {
          this.dialog = false;
          this.$swal('Success', 'Hierarchy added successfully.', 'success').then(() => {
            this.getHierarchies();
          });
        })
          .catch(error => {
            console.error('Error adding hierarchy:', error);
            this.$swal('Failed to add hierarchy', error.response.data.message, 'error');
          });
      },
      editHierarchy(item) {
        this.editedIndex = this.hierarchies.indexOf(item);
        this.newHierarchy = Object.assign({}, item);

        this.getApproversByRole(this.getRoleByApprovalLevel(this.newHierarchy.approvalLevel))
          .then(() => {
            this.newHierarchy.emailRecipient = item.emailRecipient;
          });

        this.dialog = true;
      },
      updateHierarchy() {
        const isSalesHead = this.newHierarchy.approvalLevel === 2;
        this.$axios.post(`${this.$config.restUrl}/api/customer/updatehierarchy`, {
          data: {
            id: this.newHierarchy.id,
            requestingTeam: this.newHierarchy.requestingTeam,
            approverTeam: this.newHierarchy.approverTeam,
            isSalesHead,
            approvalLevel: this.newHierarchy.approvalLevel,
            emailRecipient: this.newHierarchy.emailRecipient
          }
        }).then(response => {
          Object.assign(this.hierarchies[this.editedIndex], response.data);
          this.dialog = false;
          this.$swal('Success', 'Hierarchy updated successfully.', 'success').then(() => {
            this.getHierarchies();
          });
        })
          .catch(error => {
            console.error('Error updating hierarchy:', error);
            this.$swal('Failed to update hierarchy', error.response.data.message, 'error');
          });
      },
      deleteHierarchy(id) {
        this.$swal({
          title: 'Are you sure?',
          text: "This action cannot be undone!",
          icon: 'warning',
          showCancelButton: true,
          confirmButtonColor: '#3085d6',
          cancelButtonColor: '#d33',
          confirmButtonText: 'Yes, delete it!'
        }).then((result) => {
          if (result.isConfirmed) {
            this.$axios.post(`${this.$config.restUrl}/api/customer/deletehierarchy`, id, {
              headers: { 'Content-Type': 'application/json' }
            })
              .then(response => {
                this.$swal('Deleted', 'Hierarchy has been deleted.', 'success').then(() => {
                  this.getHierarchies();
                });
              })
              .catch(error => {
                console.error('Error deleting hierarchy:', error);
                this.$swal('Failed to delete hierarchy', error.response.data.message, 'error');
              });
          }
        });
      }
    },
  };
</script>

<style scoped>
  @import '~@/../wwwroot/css/general-table.css';
</style>
