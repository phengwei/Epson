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
          <v-text-field v-model="searchTerm"
                        append-icon="mdi-magnify"
                        placeholder="Search by end user or request #"
                        solo
                        hide-details
                        flat
                        dense
                        class="search-bar"
                        @keyup.enter="triggerSearch"
                        @click:append="triggerSearch"></v-text-field>
        </v-toolbar>
      </template>

      <!-- Action column slot -->
      <template v-slot:item.action="{ item }">
        <v-btn @click="redirectToQuotation(item)">ACTION</v-btn>
      </template>
    </v-data-table>
  </v-app>
</template>

<script>
  import moment from "moment";

  export default {
    name: "ItemsPendingMakerDecision",
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
          .get(`${this.$config.restUrl}/api/serviceRequest/GetPendingMakerItems`, { params })
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
            console.error("Error fetching maker items:", error);
          });
      },

      /**
       * Redirects to the Quotation Form with requestId and view=true
       */
      redirectToQuotation(item) {
        this.$router.push({
          path: "/serviceQuotation",
          query: {
            id: item.id,
            maker: true,
            view: true
          },
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
</style>
