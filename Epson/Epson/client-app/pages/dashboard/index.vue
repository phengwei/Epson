<template>
  <main class="mt-22">
    <div class="flex flex-row gap-2 justify-around pt-6" v-if="loggedInUser.roles.includes('Product')">
      <div>
        <v-app>
          <!-- <ItemsPendingFulfilmentTable /> -->
        </v-app>
      </div>
      <div>
        <v-app>
          <ProductTable />
        </v-app>
      </div>
    </div>
  </main>
</template>

<script>
import { mapGetters } from 'vuex';
import ProductTable from '~/components/ProductTable.vue';
// import ItemsPendingFulfilmentTable from '~/components/ItemsPendingFulfilmentTable.vue';

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
      ProductTable,
      // ItemsPendingFulfilmentTable
    }
}
</script>

<style>

</style>