<template>
  <div class="d-flex justify-content-center align-items-center vh-100" data-app="true">
    <v-card class="mx-auto card-round" style="width: 90%; padding: 20px;">
      <v-toolbar flat>
        <v-toolbar-title>
          <h2 class="blue-text big-bold">Service Requests</h2>
        </v-toolbar-title>
        <v-spacer></v-spacer>



        <!-- Search Field -->
        <v-text-field v-model="searchTerm"
                      append-icon="mdi-magnify"
                      placeholder="Search by request # or owner"
                      solo
                      hide-details
                      flat
                      dense
                      class="search-bar"
                      @keyup.enter="triggerSearch"
                      @click:append="triggerSearch"></v-text-field>

        <!-- Create Request Button -->
        <v-btn class="mr-3" @click="createRequest">
          Create Request
        </v-btn>

      </v-toolbar>

      <v-card-text>
        <!-- Data Table -->
        <v-data-table :headers="headers"
                      :items="requests"
                      :options.sync="options"
                      :items-per-page="options.itemsPerPage"
                      :footer-props="{
            itemsPerPageOptions: [5, 10, 20, 30, 50, { text: 'All', value: -1 }],
            'items-per-page-text': 'Rows per page:',
            'items-per-page-align': 'right',
          }"
                      :loading="loading"
                      :server-items-length="totalItems"
                      @update:options="updateOptions"
                      class="elevation-1">
          <!-- Action buttons in the table -->
          <template v-slot:item.action="{ item }">
            <v-btn @click="viewRequest(item)">View</v-btn>
          </template>
        </v-data-table>
      </v-card-text>
    </v-card>
  </div>
</template>

<script>
  import { mapGetters } from 'vuex';
  import moment from 'moment';

  export default {
    name: 'ServiceRequestQuotation',
    data() {
      return {
        headers: [
          { text: 'Request #', value: 'id', align: 'center', sortable: false },
          { text: 'Owner', value: 'owner', align: 'center', sortable: false },
          { text: 'Status', value: 'status', align: 'center', sortable: false },
          { text: 'Reported Date', value: 'reportedDate', align: 'center', sortable: false },
          { text: 'Requester', value: 'requesterName', align: 'center', sortable: false },
          { text: 'Category', value: 'category', align: 'center', sortable: false },
          { text: 'Created On', value: 'createdOnUTC', align: 'center', sortable: false },
          { text: 'Actions', value: 'action', align: 'center', sortable: false },
        ],
        requests: [],
        options: {
          page: 1,
          itemsPerPage: 10,
          sortBy: [],
          sortDesc: [],
        },
        totalItems: 0,
        loading: false,
        searchTerm: '',
      };
    },
    computed: {
      ...mapGetters(['isAuthenticated', 'loggedInUser']),
    },
    created() {
      this.getRequests();
    },
    methods: {
      /**
       * Fetch the list of service requests from your API.
       */
      getRequests() {
        this.loading = true;
        const params = {
          search: this.searchTerm,
          page: this.options.page,
          itemsPerPage: this.options.itemsPerPage,
        };
        this.$axios
          .get(`${this.$config.restUrl}/api/serviceRequest/GetServiceRequests`, { params })
          .then((response) => {
            // Map the data to format dates, etc.
            this.requests = response.data.data.map((item) => {
              return {
                ...item,
                createdOnUTC: moment(item.createdOnUTC).format('DD MMM YY HH:mm'),
                reportedDate: moment(item.reportedDate).format('DD MMM YY HH:mm'),
              };
            });
            this.totalItems = response.data.count;
            this.loading = false;
          })
          .catch((error) => {
            this.loading = false;
            console.error('Error fetching service requests:', error);
          });
      },

      /**
       * Trigger a new search.
       */
      triggerSearch() {
        this.options.page = 1;
        this.getRequests();
      },

      /**
       * Update table options.
       */
      updateOptions(newOptions) {
        this.options = newOptions;
        this.getRequests();
      },

      /**
       * View request details. Pass the id in the query.
       */
      viewRequest(request) {
        this.$router.push({
          path: '/serviceQuotation',
          query: { id: request.id, view: true },
        });
      },

      /**
       * Redirect to Create Request page.
       */
      createRequest() {
        this.$router.push({
          path: '/serviceQuotation',
        });
      },
    },
  };
</script>

<style scoped>
  .vh-100 {
    height: 100vh;
  }

  .search-bar {
    width: 250px;
    border-radius: 20px;
    padding: 5px 10px;
  }

  .blue-text {
    color: #003399 !important;
  }

  .big-bold {
    font-size: 24px;
    font-weight: bold;
  }

  .card-round {
    border-radius: 20px !important;
  }
</style>
