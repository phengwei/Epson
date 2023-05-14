<template>
  <nav class="bg-[#19212b] top-0 inset-x-0 w-full z-30 text-white fixed transition duration-300 delay-0 ease-out" :class="{'transition-right': showPopup}">
    <div class="flex sm:px-18 px-4">
      <div class="w-full flex justify-between flex-row-reverse">
        <div class="flex md:space-x-7 md:w-full">
          <!-- Website Logo -->
          <div class="w-full">
            <nuxt-link to="/" class="flex items-center py-4 px-2 w-[150px]">
              <img class="w-[150px] h-14 object-contain"
                   src="/images/svg/epson-logo.png"
                   alt="Epson" />
            </nuxt-link>
          </div>
          <!-- Primary Navbar items -->
          <div class="hidden md:flex items-center w-full justify-end " v-if="isAuthenticated">
            <nuxt-link to="/user"
                       class="w-40  h-full hover:bg-[#003399] flex justify-center items-center  font-semibold transition duration-300">{{ loggedInUser.userName }}</nuxt-link>
            <nuxt-link to="/reporting"
                       class="w-40  h-full hover:bg-[#003399] flex justify-center items-center font-semibold transition duration-300">Report</nuxt-link>

            <nuxt-link to="/slaDashboard"
                       class="w-40  h-full hover:bg-[#003399] flex justify-center items-center font-semibold transition duration-300">SLA Overview</nuxt-link>

            <!-- to be moved to admin configuration screen in the future -->
            <nuxt-link to="/sla"
                       class="w-40  h-full hover:bg-[#003399] flex justify-center items-center font-semibold transition duration-300">SLA Configuration</nuxt-link>

            <a class="w-40  h-full hover:bg-[#003399] flex justify-center items-center font-semibold transition duration-300 cursor-pointer"
               target="_blank"
               @click="logout">Logout</a>
          </div>
        </div>
        <!-- Mobile menu button -->
        <div class="md:hidden flex items-center">
          <button class="outline-none" @click="openPopup()">
            <svg class="w-6 h-6 text-white"
                 x-show="!showMenu"
                 fill="none"
                 stroke-linecap="round"
                 stroke-linejoin="round"
                 stroke-width="2"
                 viewBox="0 0 24 24"
                 stroke="currentColor">
              <path d="M4 6h16M4 12h16M4 18h16"></path>
            </svg>
          </button>
        </div>
      </div>
    </div>
  </nav>
</template>

<script>
import { mapGetters } from 'vuex';
import Vue from 'vue'
import EventBus from '~/components/eventbus'

Vue.directive('scroll', {
    inserted (el, binding) {
        const f = function (event) {
            if (binding.value(event, el)) {
                window.removeEventListener('scroll', f)
            }
        }
        window.addEventListener('scroll', f)
    }
})
export default {
    name: 'HeaderNav',
    computed: {
        ...mapGetters(['isAuthenticated', 'loggedInUser'])
    },
    data() {
        return {
            showMobileMenu: false,
            showPopup: false,
        }
    },
    components: {
    },
    methods: {
        updateScroll () {
            this.scrollPosition = window.scrollY
        },
        openPopup () {
            this.showPopup = true;
            window.scrollTo(0, 0);
            document.body.classList.add('stop-scrolling')
            EventBus.$emit('OPEN_MOBILE_HEADER', this.showPopup)
        },
        async logout(){
            await this.$auth.logout().then(response => {
                this.$router.push("/login");
            })
        }
    },
    mounted() {
        console.log('epson-header-mounted');
        console.log('auth user', this.$auth.state.user);
        
        const $vm = this
        EventBus.$on('OPEN_MOBILE_HEADER', function (showPopup) {
            $vm.showPopup = showPopup
        })
        EventBus.$on('CLOSE_MOBILE_HEADER', function (showPopup) {
            $vm.showPopup = showPopup
        })
    }
}
</script>

<style>
.homeHeader {
    background-color: #f1f3f5;
    color: rgb(107 114 128);
}
.homePageMobile {
    background-color: #f1f3f5;
}
.homePageIsActive {
    color: #003399;
    border-color: #003399;
    border-bottom-width: 4px;
}

.homePageIsActiveScrolled {
    color: #003399;
    border-color: #003399;
}

.homePageIsActiveMobile {
    background-color: #003399;
    color:white
}

.stop-scrolling {
  height: 100%;
  overflow: hidden
}

.transition-right{
    transform: translate(100%, 0);
}
</style>
