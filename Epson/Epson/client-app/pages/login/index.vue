<template>
  <main class="w-full h-full flex items-center justify-center">
    <div class="absolute top-0 w-full h-full bg-blue-800"></div>
    <div class="absolute top-0 right-0 mr-4 mt-4">
      <nuxt-link to="/" class="flex items-center py-4 px-2">
        <img class="w-[150px] h-14 object-contain" src="/images/svg/navbar/epson-logo.png" alt="Epson" />
      </nuxt-link>
    </div>
    <div class="container mx-auto px-4 h-full">
      <div class="flex content-center items-center justify-center h-full">
        <div class="w-full lg:w-4/12 px-4">
          <div class="relative flex flex-col min-w-0 break-words w-full mb-6 shadow-lg rounded-lg bg-white border-0">
            <div class="rounded-t mb-0 px-6 py-6">
              <div class="text-center mb-3">
                <h6 class="text-blue-800 text-2xl font-bold">
                  LOGIN
                </h6>
              </div>
              <hr class="mt-6 border-b-1 border-blue-800" />
            </div>
            <div class="flex-auto px-4 lg:px-10 py-10 pt-0">
              <form method="post" @submit.prevent="login">
                <div class="relative w-full mb-3">
                  <label class="block uppercase text-blue-800 text-xs font-bold mb-2"
                         for="grid-username">Username</label>
                  <input type="text"
                         class="border-0 px-3 py-3 placeholder-gray-400 text-blue-800 bg-white rounded text-sm shadow focus:outline-none focus:ring w-full"
                         placeholder="Username"
                         style="transition: all 0.15s ease 0s;"
                         v-model="userName" />
                </div>
                <div class="relative w-full mb-3">
                  <label class="block uppercase text-blue-800 text-xs font-bold mb-2"
                         for="grid-password">Password</label>
                  <input type="password"
                         class="border-0 px-3 py-3 placeholder-gray-400 text-blue-800 bg-white rounded text-sm shadow focus:outline-none focus:ring w-full"
                         placeholder="Password"
                         style="transition: all 0.15s ease 0s;"
                         v-model="password" />
                </div>
                <div class="text-center mt-6">
                  <button :disabled="loginDisabled"
                          class="w-full bg-white text-blue-800 border border-blue-800 hover:bg-blue-800 hover:text-white hover:border-blue-800 active:bg-blue-800 active:text-white active:border-blue-800 text-sm font-bold uppercase px-6 py-3 rounded-full shadow hover:shadow-lg outline-none focus:outline-none flex items-center justify-center mx-auto"
                          type="submit"
                          style="transition: all 0.15s ease 0s;">
                    <span class="mr-2">Sign In</span>
                    <i class="fas fa-arrow-right"></i>
                  </button>
                </div>
              </form>
              <div class="text-center mt-6">
                <a href="/auth/ssoLogin"
                   class="w-full bg-white text-blue-800 hover:bg-blue-800 hover:text-white active:bg-blue-800 active:text-white text-sm font-bold uppercase px-6 py-3 rounded-full shadow hover:shadow-lg outline-none focus:outline-none flex items-center justify-center mx-auto"
                   style="transition: all 0.15s ease 0s;">
                  <span class="mr-2">SSO Login</span>
                  <i class="fas fa-building"></i>
                </a>
              </div>
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
    components: {},
    data() {
      return {
        userName: '',
        password: '',
        error: null,
        loginDisabled: false,
      }
    },
    head() {
      return {
        title: "GCS Login"
      }
    },
    beforeMount() {
      if (this.$auth.loggedIn) {
        this.$router.push('/dashboard');
      }
    },
    mounted() { },
    destroyed() { },
    methods: {
      async login() {
        try {
          this.loginDisabled = true;
          const response = await this.$axios.post('/api/customer/login', {
            data: {
              userName: this.userName,
              password: this.password
            }
          });

          if (response.data.isShowQR) {
            this.$router.push({
              path: '/qr',
              query: { qrCode: response.data.qrCode, email: response.data.email }
            });
          } else {
            this.$router.push({
              path: '/validateTwoFactor',
              query: { email: response.data.email }
            });
          }
        } catch (error) {
          this.loginDisabled = false;
          const errorMessage = error.response.data.error;
          Swal.fire({
            title: 'Login Unsuccessful!',
            text: errorMessage,
            icon: 'error',
            confirmButtonText: 'OK'
          });
        } finally {
          this.loginDisabled = false;
        }
      }
    }
  }
</script>

<style scoped>
  @import url('https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.3/css/all.min.css');

  body {
    background-color: #1C3FAA;
  }

  button:disabled {
    background-color: #e0e0e0;
    color: #aaa;
    cursor: not-allowed;
  }

    button:disabled .fas {
      color: #aaa;
    }
</style>
