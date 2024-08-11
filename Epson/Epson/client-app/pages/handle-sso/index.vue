<template>
  <div class="flex justify-center items-center h-screen">
    <div class="text-center">
      <h1 class="text-3xl font-semibold mb-4">Logging you in...</h1>
      <p>Please wait while we complete the login process.</p>
    </div>
  </div>
</template>

<script>
  import Swal from 'sweetalert2';
  export default {
    name: "ssoHandle",
    auth: false,
    async mounted() {
      const token = this.$route.query.token;
      const returnUrl = this.$route.query.returnUrl || '/dashboard';

      if (token) {
        try {
          this.$auth.setUserToken(token);

          await this.$auth.fetchUser();
          this.$router.push(returnUrl);
        } catch (error) {
          console.error("Error during SSO login handling:", error);
          Swal.fire({
            title: 'Login Error',
            text: 'An error occurred while logging you in. Please try again.',
            icon: 'error',
            confirmButtonText: 'OK'
          });
          this.$router.push('/login');
        }
      } else {
        Swal.fire({
          title: 'Login Error',
          text: 'No token found in the SSO response.',
          icon: 'error',
          confirmButtonText: 'OK'
        });
        this.$router.push('/login');
      }
    }
  }
</script>
