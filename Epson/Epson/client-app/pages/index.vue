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
        this.$router.push('/userManagement');
      } else if (userRoles.includes('Product') || userRoles.includes('Coverplus')) {
        this.$router.push('/productDashboard');
      } else if (userRoles.includes('Sales')) {
        this.$router.push('/salesDashboard');
      } else if (userRoles.includes('Sales Section Head')) {
        this.$router.push('/request');
      } else {
        this.$router.push('/reporting'); 
      }
    }
  }
</script>

