<template>
  <main class="w-full h-full flex items-center justify-center">
    <div class="absolute top-0 w-full h-full bg-gray-900"></div>
    <div class="container mx-auto px-4 h-full">
      <div class="flex content-center items-center justify-center h-full">
        <div class="w-full lg:w-4/12 px-4">
          <div class="relative flex flex-col min-w-0 break-words w-full mb-6 shadow-lg rounded-lg bg-gray-300 border-0">
            <div class="rounded-t mb-0 px-6 py-6">
              <div class="text-center mb-3">
                <h6 class="text-gray-900 text-2xl font-bold">
                  Login
                </h6>
              </div>
              <hr class="mt-6 border-b-1 border-gray-400" />
            </div>
            <div class="flex-auto px-4 lg:px-10 py-10 pt-0">
              <form method="post" @submit.prevent="login">
                <div class="relative w-full mb-3">
                  <label class="block uppercase text-gray-700 text-xs font-bold mb-2"
                         for="grid-password">Username</label><input type="text"
                                                                    class="border-0 px-3 py-3 placeholder-gray-400 text-gray-700 bg-white rounded text-sm shadow focus:outline-none focus:ring w-full"
                                                                    placeholder="Username"
                                                                    style="transition: all 0.15s ease 0s;"
                                                                    v-model="userName" />
                </div>
                <div class="relative w-full mb-3">
                  <label class="block uppercase text-gray-700 text-xs font-bold mb-2"
                         for="grid-password">Password</label><input type="password"
                                                                    class="border-0 px-3 py-3 placeholder-gray-400 text-gray-700 bg-white rounded text-sm shadow focus:outline-none focus:ring w-full"
                                                                    placeholder="Password"
                                                                    style="transition: all 0.15s ease 0s;"
                                                                    v-model="password" />
                </div>
                <div class="text-center mt-6">
                  <button class="bg-gray-900 text-white active:bg-gray-700 text-sm font-bold uppercase px-6 py-3 rounded shadow hover:shadow-lg outline-none focus:outline-none mr-1 mb-1 w-full"
                          type="submit"
                          style="transition: all 0.15s ease 0s;">
                    Sign In
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
  </main>
</template>



<script>

  import Swal from 'sweetalert2';
  export default {

    name: "auth-login",
    middleware: 'guest',
    auth: false,
    components: {
    },

    data() {
      return {
        userName: '',
        password: '',
        error: null,
      }
    },
    head() {
      return {
        title: "Epson Unity Management Login"
      }
    },
    beforeMount() {
      if (this.$auth.loggedIn) {
        this.$router.push('/reporting');
      }
    },
    mounted() {

    },
    destroyed() {
    },
    methods: {

      async login() {
        try {
          await this.$auth.loginWith('local', {
            data: {
              data: {
                userName: this.userName,
                password: this.password
              }
            }
          }).then(response => {
            const userRoles = this.$auth.user.data.roles;
            console.log("roles", this.$auth.user.data.roles);
            if (userRoles.includes('Admin')) {
              this.$router.push('/userManagement');
            } else if (userRoles.includes('Product')) {
              this.$router.push('/productDashboard');
            } else if (userRoles.includes('Sales')) {
              this.$router.push('/salesDashboard');
            } else if (userRoles.includes('Coverplus')) {
              this.$router.push('/productDashboard');
            } else if (userRoles.includes('Sales Section Head')) {
              this.$router.push('/request');
            } else {
              Swal.fire({
                title: 'Error!',
                text: 'Unknown user role',
                icon: 'error',
                confirmButtonText: 'OK'
              });
            }
          }).catch(function (error) {
            console.log(error)
            Swal.fire({
              title: 'Error!',
              text: 'Login failed',
              icon: 'error',
              confirmButtonText: 'OK'
            });
          });
        } catch (err) {
          console.log(err);
        }
      }

    }

  }
</script>

