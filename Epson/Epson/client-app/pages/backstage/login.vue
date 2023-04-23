<template>
  <div class="flex justify-center items-center mt-52">
    <div class="w-full max-w-xs">

      <form class="bg-white shadow-md rounded px-8 pt-6 pb-8 mb-4" method="post" @submit.prevent="login">
        <div class="identity-input mb-4">
          <label
            for="identity"
            class="block text-gray-700 text-sm font-bold mb-2"
          >
            Username</label
          >
          <input
            id="identity"
            class="shadow appearance-none borderrounded w-full py-2 px-3 text-gray-700 mb-3 leading-tight focus:outline-none focus:shadow-outline"
            type="text"
            placeholder="Username"
            aria-describedby="usernameHelp"
            v-model="username"
          />
          <span class="text-xs text-red-700" id="emailHelp"></span>
        </div>

        <div class="password-input mb-6">
          <label
            for="identity"
            class="block text-gray-700 text-sm font-bold mb-2"
            >Password</label
          >

          <input
            aria-describedby="passwordHelp"
            v-model="password"

            class="shadow appearance-none borderrounded w-full py-2 px-3 text-gray-700 mb-3 leading-tight focus:outline-none focus:shadow-outline"
            id="password"
            type="password"
            placeholder="*******"
          />

          <span class="text-xs text-red-700" id="passwordHelp"></span>
        </div>

        <div class="flex items-center justify-between">
          <button
            class="bg-blue-600 hover:bg-black text-white font-bold py-2 px-4 rounded focus:outline-none focus:shadow-outline"
            type="submit"
          >
            Sign In
          </button>
         <!-- <nuxt-link class="inline-block align-baseline font-bold text-sm text-blue-500 hover:text-blue-800" :to="localePath('/backstage/register')" >Register</nuxt-link> -->
        </div>
      </form>
    </div>
  </div>
</template>

<script>
// import Notification from '~/components/Notification'

export default {
  name :"auth-login",
  middleware: 'guest',


  data() {
    return {
      username: '',
      password: '',
      error: null
    }
  },

  methods: {
       async login() {
         const vm = this;
      try {
        // let response = await this.$auth.loginWith('local', { data: this.login })
       await  this.$auth.loginWith('customStrategy', { data: {
          username: this.username,
          password: this.password
          } }).then(response => {
              console.log("res", response);
              /**
            this.$auth.setUser(response.data.user);
            this.$auth.setUserToken(response.data.jwtToken, response.data.token)
            // this.$auth.fetchUser();
            this.$auth.refreshTokens();
             */
        }) .catch(function (error) {
   
            console.log(error);
            console.log(error.response);
            vm.$swal('Login Failed', error.response.data.message, 'error');
      });


      } catch (err) {
        console.log(err)
      }
    },
    async loginbk() {
      try {
        await this.$auth.loginWith('local', {
          data: {
          username: this.username,
          password: this.password
          }
        })

        // this.$router.push('/backstage/dashboard')
      } catch (e) {
        this.error = e.response.data.message
      }
    }
  }
}
</script>
