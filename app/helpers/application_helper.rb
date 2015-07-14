module ApplicationHelper
	
  def full_title(page_title)
    base_title = "Олька"
    if page_title.empty?
      base_title
    else
      "#{base_title} | #{page_title}"
    end
  end

  #def check_user 
  #if current_user != nil
 	  #    if current_user.id == article.user_id || (current_user.admin? )
     #    check_it = true

     #   else check_it = false
     #   end
            
  #end
#end


end
