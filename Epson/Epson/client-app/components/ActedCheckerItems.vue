<template>
  <v-data-table :headers="headers"
                :items="actedRequests"
                :loading="loading"
                :server-items-length="totalItems"
                :items-per-page="paginationOptions.itemsPerPage"
                :options.sync="paginationOptions"
                :footer-props="{ 'items-per-page-options': [5, 10, 20, 30, 50, { text: 'All', value: -1 }] }"
                @update:options="updateOptions"
                class="elevation-1">
    <template v-slot:top>
      <v-toolbar flat>
        <v-toolbar-title class="blue-text big-bold">ACTED CHECKER REQUESTS</v-toolbar-title>
        <v-spacer></v-spacer>
        <v-text-field v-model="searchTerm"
                      append-icon="mdi-magnify"
                      placeholder="Search by request # or requested by"
                      solo
                      hide-details
                      flat
                      dense
                      class="search-bar"
                      @keyup.enter="triggerSearch"
                      @click:append="triggerSearch"></v-text-field>
      </v-toolbar>
    </template>

    <template v-slot:item="{ item }">
      <tr>
        <td>{{ item.id }}</td>
        <td>{{ item.owner }}</td>
        <td>{{ item.status }}</td>
        <td>{{ item.reportedDate }}</td>
        <td>{{ item.requester }}</td>
        <td>{{ item.category }}</td>
        <td>{{ item.createdOnUTC }}</td>
        <td>
          <v-btn @click="viewRequest(item)">View</v-btn>
        </td>
      </tr>
    </template>
  </v-data-table>
</template>

<script>
  import moment from 'moment';

  export default {
    name: 'ActedCheckerRequests',
    data() {
      return {
        headers: [
          { text: 'Request #', align: 'start', value: 'id', sortable: false },
          { text: 'Owner', align: 'start', value: 'owner', sortable: false },
          { text: 'Status', align: 'start', value: 'status', sortable: false },
          { text: 'Reported Date', align: 'start', value: 'reportedDate', sortable: false },
          { text: 'Requester', align: 'start', value: 'requester', sortable: false },
          { text: 'Category', align: 'start', value: 'category', sortable: false },
          { text: 'Created On', align: 'start', value: 'createdOnUTC', sortable: false },
          { text: 'Actions', value: 'actions', sortable: false },
        ],
        actedRequests: [],
        searchTerm: '',
        search: '',
        totalItems: 0,
        loading: true,
        paginationOptions: {
          page: 1,
          itemsPerPage: 10,
          sortBy: [],
          sortDesc: [],
        },
      };
    },
    computed: {
      query() {
        return {
          page: this.paginationOptions.page,
          itemsPerPage: this.paginationOptions.itemsPerPage,
          search: this.search,
        };
      }
    },
    created() {
      this.getActedCheckerRequests();
    },
    methods: {
      async getActedCheckerRequests() {
        this.loading = true;
        try {
          const response = await this.$axios.get(`${this.$config.restUrl}/api/serviceRequest/GetActedCheckerRequest`);

          // Ensure response contains a valid data array
          const requestData = Array.isArray(response.data.data) ? response.data.data : [];

          this.actedRequests = requestData.map(request => ({
            ...request,
            createdOnUTC: moment(request.createdOnUTC).format('DD MMM YY HH:mm'),
            reportedDate: moment(request.reportedDate).format('DD MMM YY HH:mm') // ✅ Formatting `reportedDate`
          }));

          this.totalItems = this.actedRequests.length;
        } catch (error) {
          console.error('Error fetching acted checker requests:', error);
        } finally {
          this.loading = false;
        }
      },

      viewRequest(request) {
        this.$router.push({
          path: '/serviceQuotation',
          query: {
            id: request.id, // ✅ Fix ReferenceError by replacing `item.id` with `request.id`
            view: true
          },
        });
      },

      triggerSearch() {
        this.search = this.searchTerm;
        this.paginationOptions.page = 1;
        this.getActedCheckerRequests();
      },

      updateOptions(options) {
        this.paginationOptions = options;
        this.getActedCheckerRequests();
      }
    }
  };
</script>

<style scoped>
  @import '~@/../wwwroot/css/general-table.css';
</style>
