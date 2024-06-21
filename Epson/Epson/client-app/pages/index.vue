<template>
  <div>
    Welcome to the home page.
  </div>
</template>



<script>

  export default {
    name: 'IndexApp',
    beforeMount() {
      const userRoles = this.$auth.user.data.roles;
      if (userRoles.includes('Admin')) {
        this.$router.push('/dashboard');
      } else if (userRoles.includes('Sales Operation')) {
        this.$router.push('/request');
      } else if (userRoles.includes('Product') || userRoles.includes('Coverplus')) {
        this.$router.push('/dashboard');
      } else if (userRoles.includes('Sales')) {
        this.$router.push('/dashboard');
      } else if (userRoles.includes('Sales Section Head')) {
        this.$router.push('/dashboard');
      } else if (userRoles.includes('Director')) {
        this.$router.push('/dashboard');
      } else {
        this.$router.push('/report'); 
      }
    }
  }
</script>

