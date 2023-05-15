<template>
    <!-- Mobile menu -->
    <transition
        enter-active-class="transform transition duration-500 ease-custom"
        enter-class="-translate-x-1/2 scale-x-0 opacity-0"
        enter-to-class="translate-x-0 scale-x-100 opacity-100"
        leave-active-class="transform transition duration-300 ease-custom"
        leave-class="translate-x-0 scale-x-100 opacity-100"
        leave-to-class="-translate-x-1/2 scale-x-0 opacity-0"
    >
        <div v-show="showPopup" @close-modal="closeModal" class="w-screen h-screen">
            <div class="modal-overlay flex fixed inset-0 justify-center items-center sm:items-start w-screen h-screen bg-[#19212b] z-40" >
                <div class="modal-overlay absolute w-full h-full bg-#19212b opacity-95"></div>

                <div class="modal flex flex-col relative text-center w-full h-full overflow-y-auto" @click.stop>
                
                    <div class="modal-header fixed inset-x-0 top-0 h-16 w-full bg-[#19212b] z-30 border-b-[1px] border-white">
                        <div class="modal-close absolute top-0 left-0 cursor-pointer flex flex-col items-center mt-4 ml-4 text-white text-sm z-50">
                        <div class="w-6 h-6">
                            <button @click="closeModal">
                                <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" fill="currentColor" class="bi bi-x" viewBox="0 0 16 16"> <path d="M4.646 4.646a.5.5 0 0 1 .708 0L8 7.293l2.646-2.647a.5.5 0 0 1 .708.708L8.707 8l2.647 2.646a.5.5 0 0 1-.708.708L8 8.707l-2.646 2.647a.5.5 0 0 1-.708-.708L7.293 8 4.646 5.354a.5.5 0 0 1 0-.708z"/> </svg>
                            </button>
                        </div>
                        </div>
                    </div>
                    <div class="modal-content container mx-auto h-auto mt-16 mb-16 text-center">
                        <!--Body-->
                        <ul class="text-left">
                            <li><nuxt-link to="/" class="block  px-2 py-8 text-white hover:text-white hover:bg-[#003399] font-semibold transition duration-300">{{ loggedInUser.userName }}</nuxt-link></li>
                            <hr />
                            <li><nuxt-link to="/dashboard" class="block text-white font-semibold px-2 py-8 hover:text-white hover:bg-[#003399] transition duration-300">Dashboard</nuxt-link></li>
                            <hr />
                            <li><a target="_blank" class="block px-2 text-white font-semibold py-8 hover:text-white hover:bg-[#003399] transition duration-300" @click="logout">Logout</a></li>
                            <hr />
                        </ul>
                    </div>
                </div>
            </div>
        </div>
    </transition>
    
</template>

<script>
import { mapGetters } from 'vuex';
import EventBus from '~/components/eventbus'

export default {
    name: 'MobileNav',
    data () {
      return {
        showPopup: false
      }
    },
    computed: {
        ...mapGetters(['isAuthenticated', 'loggedInUser'])
    },
    methods: {
        closeModal () {
            this.showPopup = false
            document.body.classList.remove('stop-scrolling')
            EventBus.$emit('CLOSE_MOBILE_HEADER', this.showPopup)
        },
        async logout(){
            await this.$auth.logout().then(response => {
                this.$router.push("/login");
            })
        }
    },
    mounted () {
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
.stop-scrolling {
  height: 100%;
  overflow: hidden
}
.fa-facebook-f{
    color: #2D609B;
}

.homePageIsActiveMobile {
    background-color: #003399;
    color:white
}
</style>