<template>
  <main class="w-full h-full flex items-center justify-center">
    <div class="fixed inset-0 bg-blue-800"></div>
    <div class="absolute top-0 right-0 mr-4 mt-4">
      <nuxt-link to="/" class="flex items-center py-4 px-2">
        <img class="w-[150px] h-14 object-contain" src="/images/svg/navbar/epson-logo.png" alt="Epson" />
      </nuxt-link>
    </div>

    <div class="container mx-auto px-4 h-full z-10">
      <div class="flex content-center items-center justify-center h-full">
        <div class="w-full lg:w-4/12 px-4">
          <div class="relative flex flex-col min-w-0 break-words w-full mb-6 shadow-lg rounded-lg bg-white border-0">
            <div class="rounded-t mb-0 px-6 py-6">
              <div class="text-center mb-3">
                <h6 class="text-blue-800 text-2xl font-bold">
                  Scan QR Code for 2FA
                </h6>
              </div>
              <hr class="mt-6 border-b-1 border-blue-800" />
            </div>

            <div class="flex-auto px-4 lg:px-10 py-10 pt-0 text-center">
              <img :src="qrCode" alt="2FA QR Code" class="mx-auto mb-4" />
              <p class="text-gray-600 mt-4">
                Scan the QR code with your Microsoft authenticator app.
              </p>
              <div class="text-center mt-6">
                <button @click="goToValidateTwoFactor" class="w-full bg-blue-800 text-white text-sm font-bold uppercase px-6 py-3 rounded-full shadow hover:shadow-lg outline-none focus:outline-none">
                  Next
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </main>
</template>

<script>
  export default {
    name: "qr-code",
    auth: false,
    data() {
      return {
        qrCode: this.$route.query.qrCode || '', 
        email: this.$route.query.email || ''    
      };
    },
    methods: {
      goToValidateTwoFactor() {
        this.$router.push({ path: '/validateTwoFactor', query: { email: this.email } });
      }
    }
  };
</script>

<style scoped>
  @import url('https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.3/css/all.min.css');

  body {
    background-color: #1C3FAA;
  }

  .qr-image {
    width: 100%;
    max-width: 200px;
    margin: 0 auto;
  }
</style>
