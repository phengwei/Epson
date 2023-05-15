<template>
  <v-app>
    <request-quotation />
  </v-app>
</template>
<script>
  import RequestQuotation from '~/components/RequestQuotation.vue';

  export default {
    name: "App",
    components: {
      RequestQuotation
    }
  };
</script>
