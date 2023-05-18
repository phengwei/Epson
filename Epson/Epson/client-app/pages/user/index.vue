<template>
  <main class="mt-22">
    <div class="flex flex-row gap-2 justify-center pt-6 w-full">
      <div class="w-4/5">
        <v-app>
            <h2>Change Password</h2>
            <form class="border-2 rounded-lg shadow-lg p-2">
                <v-text-field
                v-model="currentPass"
                :error-messages="currentPassErrors"
                label="Current Password"
                required
                @input="$v.currentPass.$touch()"
                @blur="$v.currentPass.$touch()"
                type="password"
                ></v-text-field>
                <v-text-field
                v-model="newPass"
                :error-messages="newPassErrors"
                label="New Password"
                required
                @input="$v.newPass.$touch()"
                @blur="$v.newPass.$touch()"
                type="password"
                ></v-text-field>
                <v-text-field
                v-model="confirmNewPass"
                :error-messages="confirmNewPassErrors"
                label="Confirm New Password"
                required
                @input="$v.confirmNewPass.$touch()"
                @blur="$v.confirmNewPass.$touch()"
                type="password"
                ></v-text-field>

                <v-btn
                class="mr-4"
                @click="submit"
                >
                submit
                </v-btn>
                <v-btn @click="clear">
                clear
                </v-btn>
            </form>
        </v-app>
      </div>
    </div>
  </main>
</template>

<script>
  import Swal from 'sweetalert2';
  import { validationMixin } from 'vuelidate'
  import { required, minLength, sameAs } from 'vuelidate/lib/validators'
  import { mapGetters } from 'vuex';
  export default {
    name: 'UserIndex',
    mixins: [validationMixin],
    middleware: 'auth',
    validations: {
      currentPass: { required, minLength: minLength(8) },
      newPass: { required, minLength: minLength(8) },
      confirmNewPass: { sameAsPassword: sameAs('newPass')}
    },
    data () {
        return {
            currentPass: '',
            newPass: '',
            confirmNewPass: ''
        }
    },
    computed: {
        ...mapGetters(['isAuthenticated', 'loggedInUser']),
        currentPassErrors () {
            const errors = []
            if (!this.$v.currentPass.$dirty) return errors
            !this.$v.currentPass.minLength && errors.push('Password must be at least 8 characters long')
            !this.$v.currentPass.required && errors.push('Current Password is required.')
            return errors
        },
        newPassErrors () {
            const errors = []
            if (!this.$v.newPass.$dirty) return errors
            !this.$v.newPass.minLength && errors.push('New Password must be at least 8 characters long')
            !this.$v.newPass.required && errors.push('New Password is required.')
            return errors
        },
        confirmNewPassErrors () {
            const errors = []
            if (!this.$v.confirmNewPass.$dirty) return errors
            !this.$v.confirmNewPass.sameAsPassword && errors.push('Confirm New Password must be same as New Password')
            return errors
        },
    },
    methods: {
      async submit() {
        this.$v.$touch()
        if (!this.$v.$invalid) { 
          try {
            const response = await this.$axios.post(`${this.$config.restUrl}/api/customer/changepassword?currentPassword=${this.currentPass}&newPassword=${this.newPass}`);
            if (response.status === 200) {
              Swal.fire(
                'Changed!',
                'Your password has been changed.',
                'success'
              ).then(() => {
                location.reload();
              });
            } else {
              Swal.fire(
                'Failed!',
                'Failed to change your password.',
                'error'
              );
            }
          } catch (error) {
            console.error(error);
            Swal.fire(
              'Error!',
              'There was a problem changing your password.',
              'error'
            );
          }
        }
      },
      clear () {
          this.$v.$reset()
          this.currentPass = ''
          this.newPass = ''
          this.confirmNewPass = ''
      },
    }

}
</script>

<style>

</style>
