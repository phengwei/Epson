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
                  Two-factor Authentication
                </h6>
              </div>
              <hr class="mt-6 border-b-1 border-blue-800" />
            </div>

            <div class="flex-auto px-4 lg:px-10 py-10 pt-0">
              <form id="otpForm" class="needs-validation" @submit.prevent="handleOTP">
                <div class="text-center mb-3">
                  <h4>Authentication Code</h4>
                </div>
                <div class="input-field col-12 text-center mb-4">
                  <input type="text" v-model="otp[0]" @input="moveToNext(0)" @keypress="onlyNumber($event)" ref="otp0" class="otp-input" inputmode="numeric" maxlength="1" />
                  <input type="text" v-model="otp[1]" @input="moveToNext(1)" @keypress="onlyNumber($event)" ref="otp1" class="otp-input" inputmode="numeric" maxlength="1" />
                  <input type="text" v-model="otp[2]" @input="moveToNext(2)" @keypress="onlyNumber($event)" ref="otp2" class="otp-input" inputmode="numeric" maxlength="1" />
                  <input type="text" v-model="otp[3]" @input="moveToNext(3)" @keypress="onlyNumber($event)" ref="otp3" class="otp-input" inputmode="numeric" maxlength="1" />
                  <input type="text" v-model="otp[4]" @input="moveToNext(4)" @keypress="onlyNumber($event)" ref="otp4" class="otp-input" inputmode="numeric" maxlength="1" />
                  <input type="text" v-model="otp[5]" @input="moveToNext(5)" @keypress="onlyNumber($event)" ref="otp5" class="otp-input" inputmode="numeric" maxlength="1" />
                </div>

                <div class="text-center mt-6">
                  <button :disabled="!isOtpValid" class="w-full bg-blue-800 text-white text-sm font-bold uppercase px-6 py-3 rounded-full shadow hover:shadow-lg outline-none focus:outline-none">
                    Verify OTP
                  </button>
                </div>
              </form>

              <p class="mt-4 small text-center">Open the two-factor authenticator app on your mobile to view your authentication code.</p>
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
    auth: false,
    name: "validateTwoFactor",
    data() {
      return {
        otp: Array(6).fill(""),
        email: this.$route.query.email || '' // Retrieve email from route query
      };
    },
    computed: {
      isOtpValid() {
        return this.otp.every(digit => digit !== "");
      }
    },
    methods: {
      onlyNumber(event) {
        const charCode = event.which ? event.which : event.keyCode;
        if (charCode < 48 || charCode > 57) {
          event.preventDefault();
        }
      },
      async handleOTP() {
        const otpCode = this.otp.join("");
        try {
          const response = await this.$auth.loginWith('local', {
            data: {
              data: {
                OTP: otpCode,
                email: this.email
              }
            }
          });
          if (response.data.token) {
            this.$router.push('/dashboard');
          } else {
            Swal.fire({
              title: 'Error!',
              text: 'Invalid 2FA code. Please try again.',
              icon: 'error',
              confirmButtonText: 'OK'
            });
          }
        } catch (error) {
          Swal.fire({
            title: 'Error!',
            text: 'Authentication error.',
            icon: 'error',
            confirmButtonText: 'OK'
          });
        }
      },
      moveToNext(index) {
        if (this.otp[index] !== "" && this.$refs[`otp${index + 1}`]) {
          this.$refs[`otp${index + 1}`].focus();
        }
      }

    }
  };
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

  .otp-input {
    width: 2rem;
    padding: 0.5rem;
    font-size: 1.5rem;
    text-align: center;
    margin: 0 0.2rem;
    border: 1px solid #ccc;
    border-radius: 0.25rem;
  }
</style>
