<template>
  <v-app>
    <!-- Data Table -->
    <v-data-table :headers="headers"
                  :items="requests"
                  :options.sync="options"
                  :items-per-page="options.itemsPerPage"
                  :loading="loading"
                  :footer-props="{
        'items-per-page-options': [5, 10, 20, 30, 50, { text: 'All', value: -1 }],
        'items-per-page-text': 'Rows per page:',
        'items-per-page-align': 'right'
      }"
                  :server-items-length="totalItems"
                  class="elevation-1"
                  @update:options="updateOptions">
      <!-- The top slot with v-toolbar -->
      <template v-slot:top>
        <v-toolbar flat>
          <v-toolbar-title class="blue-text big-bold">NEW REQUEST</v-toolbar-title>
          <v-spacer></v-spacer>
          <!-- Search Field -->
          <!--<v-text-field v-model="searchTerm"
                        append-icon="mdi-magnify"
                        placeholder="Search by end user or request #"
                        solo
                        hide-details
                        flat
                        dense
                        class="search-bar"
                        @keyup.enter="triggerSearch"
                        @click:append="triggerSearch"></v-text-field>-->
        </v-toolbar>
      </template>

      <!-- Action column slot -->
      <template v-slot:item.action="{ item }">
        <v-btn @click="openAssignDialog(item)">Assign</v-btn>
      </template>
    </v-data-table>

    <!-- Assign Dialog -->
    <v-dialog v-model="assignDialog" max-width="500px">
      <v-card>
        <v-card-title class="blue-text">Assign Service Request to Member</v-card-title>
        <v-card-text>
          <v-select v-model="selectedUserId"
                    :items="departmentUsers"
                    item-value="id"
                    item-text="userName"
                    label="Select User"
                    outlined></v-select>
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="grey" @click="assignDialog = false">Cancel</v-btn>
          <v-btn color="blue darken-1" @click="assignRequest">Assign</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-app>
</template>

<script>
  import moment from "moment";
  import Swal from "sweetalert2"; // Import SweetAlert

  export default {
    name: "ItemsPendingManagerAction",
    data() {
      return {
        requests: [],
        totalItems: 0,
        loading: false,
        searchTerm: "",
        options: {
          page: 1,
          itemsPerPage: 10,
          sortBy: [],
          sortDesc: [],
        },
        assignDialog: false,
        selectedRequestId: null,
        selectedUserId: null,
        departmentUsers: [],

        headers: [
          { text: "Request #", value: "id", align: "center", sortable: false },
          { text: "Owner", value: "owner", align: "center", sortable: false },
          { text: "Status", value: "status", align: "center", sortable: false },
          { text: "Reported Date", value: "reportedDate", align: "center", sortable: false },
          { text: "Requester", value: "requesterName", align: "center", sortable: false },
          { text: "Category", value: "category", align: "center", sortable: false },
          { text: "Created On", value: "createdOnUTC", align: "center", sortable: false },
          { text: "Actions", value: "action", align: "center", sortable: false },
        ],
      };
    },
    watch: {
      options: {
        handler() {
          this.getPendingManagerItems();
        },
        deep: true,
      },
    },
    created() {
      this.getPendingManagerItems();
      this.getDepartmentUsers();
    },
    methods: {
      getPendingManagerItems() {
        this.loading = true;
        const params = {
          search: this.searchTerm,
          page: this.options.page,
          itemsPerPage: this.options.itemsPerPage,
        };
        this.$axios
          .get(`${this.$config.restUrl}/api/serviceRequest/GetPendingManagerItems`, { params })
          .then((response) => {
            const data = response.data.data || [];
            this.requests = data.map((item) => ({
              ...item,
              createdOnUTC: item.createdOnUTC ? moment(item.createdOnUTC).format("DD MMM YY HH:mm") : "",
              reportedDate: item.reportedDate ? moment(item.reportedDate).format("DD MMM YY HH:mm") : "",
            }));
            this.totalItems = response.data.count || 0;
            this.loading = false;
          })
          .catch((error) => {
            this.loading = false;
            console.error("Error fetching manager items:", error);
          });
      },

      getDepartmentUsers() {
        this.$axios
          .get(`${this.$config.restUrl}/api/serviceRequest/GetDepartmentUsers`)
          .then((response) => {
            this.departmentUsers = response.data.data || [];
          })
          .catch((error) => {
            console.error("Error fetching department users:", error);
          });
      },

      openAssignDialog(item) {
        this.selectedRequestId = item.id;
        this.selectedUserId = null;
        this.assignDialog = true;
      },

      assignRequest() {
        if (!this.selectedRequestId || !this.selectedUserId) return;

        this.$axios
          .post(`${this.$config.restUrl}/api/serviceRequest/AssignServiceRequestMaker`, null, {
            params: {
              requestId: this.selectedRequestId,
              newOwnerId: this.selectedUserId,
            },
          })
          .then(() => {
            this.assignDialog = false;
            Swal.fire({
              icon: "success",
              title: "Successfully Assigned!",
              text: "The service request has been assigned.",
              confirmButtonText: "OK",
            }).then(() => {
              window.location.reload(); // Reload the page after confirmation
            });
          })
          .catch((error) => {
            console.error("Error assigning request:", error);
            Swal.fire({
              icon: "error",
              title: "Error",
              text: "Failed to assign the service request. Please try again.",
            });
          });
      },

      triggerSearch() {
        this.options.page = 1;
        this.getPendingManagerItems();
      },

      updateOptions(newOptions) {
        this.options = newOptions;
        this.getPendingManagerItems();
      },
    },
  };
</script>

<style scoped>
  @import '~@/../wwwroot/css/general-table.css';
</style>
