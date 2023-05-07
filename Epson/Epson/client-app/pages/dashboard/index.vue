<template>
  <main class="mt-22">
    <div class="flex flex-row gap-2 justify-around pt-6" v-if="loggedInUser.roles.includes('Admin')">
      <div>
        <v-app>
          <CrudTable />
        </v-app>
      </div>
      <div>
        <v-app>
          <CrudTable />
        </v-app>
      </div>
    </div>
  </main>
</template>

<script>
import { mapGetters } from 'vuex';
import CrudTable from '~/components/CRUDTable.vue';

export default {
    name: "DashboardMain",
    middleware: "auth",
    computed: {
        ...mapGetters(['isAuthenticated', 'loggedInUser'])
    },
    data() {
        return {};
    },
    components: { 
      CrudTable
    }
}
</script>

<style>

</style>