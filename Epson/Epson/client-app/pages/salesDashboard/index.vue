<template>
  <v-app>
    <ItemsPendingRequesterAction />
  </v-app>
</template>
<script>
  import ItemsPendingRequesterAction from '~/components/ItemsPendingRequesterAction.vue';

  export default {
    name: "SalesDashboard",
    components: {
      ItemsPendingRequesterAction
    }
  };
</script>
