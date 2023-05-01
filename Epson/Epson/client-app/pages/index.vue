<template>
  <div>
    Welcome to the home page.
  </div>
</template>



<script>

  export default {
    name: 'IndexApp',
    created() {
      this.$router.push('/dashboard');
    }
  }
</script>

