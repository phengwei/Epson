<template>
  <div class="flex flex-col justify-center items-center h-screen bg-gray-100">
    <div class="text-center p-6 max-w-lg bg-white shadow-lg rounded-lg">
      <h1 class="text-4xl font-bold text-red-600 mb-4">Access Denied</h1>
      <p class="text-lg text-gray-700 mb-6">You are not authorized to access this page.</p>
      <p class="text-md text-gray-600 mb-8">Please contact the system administrator for further assistance.</p>
      <div class="flex justify-center">
        <router-link to="/login">
          <button class="px-6 py-2 text-white bg-red-500 hover:bg-red-600 rounded-md transition duration-300">Go to Login</button>
        </router-link>
      </div>
    </div>
  </div>
</template>

<script>
  export default {
    name: "UnauthorizedAccess",
    auth: false, 
    mounted() {
      console.warn("Unauthorized access attempted.");
    }
  };
</script>

<style scoped>
  .bg-gray-100 {
    background-color: #f7fafc;
  }
</style>
